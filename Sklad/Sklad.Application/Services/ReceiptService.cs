using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sklad.Application.Interfaces;
using Sklad.Contracts.Dtos;
using Sklad.Contracts.Requests;
using Sklad.Contracts.Responses;
using Sklad.Domain.Constants;
using Sklad.Domain.Enums;
using Sklad.Domain.Models;
using Sklad.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sklad.Application.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly SkladDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IReadOnlyDictionary<MessageKeyEnum, string> _messages;

        public ReceiptService(SkladDbContext dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
            _messages = OperationResultMessages.GetMessagesFor(typeof(ReceiptDocument));
        }
        public async Task<List<ReceiptDocument>> GetReceiptDocumentsAsync(DocumentFilterDto filters)
        {
            var documents = _dbContext.ReceiptDocuments
                .Include(d => d.ReceiptResources)
                .ThenInclude(r => r.Resource)
                .Include(d => d.ReceiptResources)
                .ThenInclude(r => r.Unit)
                .AsQueryable();

            if (filters != null)
            {
                if (filters.DocumentNumbers?.Any() == true)
                {
                    documents = documents.Where(d => filters.DocumentNumbers.Contains(d.Number));
                }
                if (filters.ResourceIds?.Any() == true)
                {
                    documents = documents.Where(d => d.ReceiptResources.Any(or => filters.ResourceIds.Contains(or.ResourceId)));
                }
                if (filters.UnitIds?.Any() == true)
                {
                    documents = documents.Where(d => d.ReceiptResources.Any(or => filters.UnitIds.Contains(or.UnitId)));
                }
                if (filters.FromDate.HasValue)
                {
                    documents = documents.Where(d => d.Date >= filters.FromDate.Value);
                }
                if (filters.ToDate.HasValue)
                {
                    documents = documents.Where(d => d.Date <= filters.ToDate.Value);
                }
            }
            return await documents.ToListAsync();
        }

        public async Task<OperationResult<ReceiptDocument>> CreateReceiptDocumentAsync(CreateReceiptDocumentRequest request)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var document = new ReceiptDocument
                {
                    Number = await GetDocumentNumber(),
                    Date = DateTime.UtcNow,
                };

                await _dbContext.ReceiptDocuments.AddAsync(document);

                if (request.Resources.Any())
                {
                    await _dbContext.ReceiptResources.AddRangeAsync(request.Resources.Select(r =>
                        new ReceiptResource
                        {
                            ReceiptDocument = document,
                            ResourceId = r.ResourceId,
                            UnitId = r.UnitId,
                            Count = r.Count
                        }));

                    foreach (var resource in request.Resources)
                    {
                        var balance = await _dbContext.Balances
                            .FirstOrDefaultAsync(b => b.ResourceId == resource.ResourceId && b.UnitId == resource.UnitId);

                        if (balance == null)
                        {
                            balance = new Balance
                            {
                                ResourceId = resource.ResourceId,
                                UnitId = resource.UnitId,
                                Count = 0
                            };
                            await _dbContext.Balances.AddAsync(balance);
                        }

                        balance.Count += resource.Count;
                    }
                }

                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return new OperationResult<ReceiptDocument>
                {
                    StatusCode = HttpStatusCode.Created,
                    Message = _messages[MessageKeyEnum.Created],
                    Data = document
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, _messages[MessageKeyEnum.CreationFailed]);
                return new OperationResult<ReceiptDocument>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = _messages[MessageKeyEnum.CreationFailed]
                };
            }
        }

        public async Task<OperationResult<ReceiptDocument>> UpdateReceiptDocumentAsync(UpdateReceiptDocumentRequest request)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var document = await _dbContext.ReceiptDocuments
                    .Include(d => d.ReceiptResources)
                    .FirstOrDefaultAsync(d => d.Id == request.DocumentId);
                if (document == null)
                {
                    return new OperationResult<ReceiptDocument>
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = _messages[MessageKeyEnum.NotFound]
                    };
                }
                if (request.Resources != null && request.Resources.Any())
                {
                    foreach (var resource in document.ReceiptResources.ToList())
                    {
                        var updatedResource = request.Resources.FirstOrDefault(r => r.ResourceId == resource.ResourceId && r.UnitId == resource.UnitId);
                        var balance = await _dbContext.Balances
                            .FirstOrDefaultAsync(b => b.ResourceId == resource.ResourceId && b.UnitId == resource.UnitId);
                        if (balance == null)
                        {
                            balance = new Balance
                            {
                                ResourceId = resource.ResourceId,
                                UnitId = resource.UnitId,
                                Count = 0
                            };
                            await _dbContext.Balances.AddAsync(balance);
                        }
                        if (updatedResource != null)
                        {
                            var delta = updatedResource.Count - resource.Count;
                            if (delta < 0)
                            {
                                var deltaToRemove = Math.Abs(delta);
                                if (balance.Count < deltaToRemove)
                                {
                                    return new OperationResult<ReceiptDocument>
                                    {
                                        StatusCode = HttpStatusCode.BadRequest,
                                        Message = _messages[MessageKeyEnum.NotEnoughResource]
                                    };
                                }

                                balance.Count -= deltaToRemove;
                            }
                            else
                            {
                                balance.Count += delta;
                            }
                            resource.Count = updatedResource.Count;

                        }
                        else
                        {
                            if (balance.Count < resource.Count)
                            {
                                return new OperationResult<ReceiptDocument>
                                {
                                    StatusCode = HttpStatusCode.BadRequest,
                                    Message = _messages[MessageKeyEnum.NotEnoughResource]
                                };
                            }
                            balance.Count -= resource.Count;
                            _dbContext.ReceiptResources.Remove(resource);
                        }
                    }
                    foreach (var newResource in request.Resources.Where(r => !document.ReceiptResources.Any(ir => ir.ResourceId == r.ResourceId && ir.UnitId == r.UnitId)))
                    {
                        var balance = await _dbContext.Balances
                            .FirstOrDefaultAsync(b => b.ResourceId == newResource.ResourceId && b.UnitId == newResource.UnitId);
                        if (balance == null)
                        {
                            _dbContext.Balances.Add(new Balance
                            {
                                ResourceId = newResource.ResourceId,
                                UnitId = newResource.UnitId,
                                Count = newResource.Count
                            });
                        }
                        _dbContext.ReceiptResources.Add(new ReceiptResource
                        {
                            ReceiptDocumentId = document.Id,
                            ResourceId = newResource.ResourceId,
                            UnitId = newResource.UnitId,
                            Count = newResource.Count
                        });
                    }
                }
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OperationResult<ReceiptDocument>
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = _messages[MessageKeyEnum.Updated],
                    Data = document
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, _messages[MessageKeyEnum.UpdateFailed]);
                return new OperationResult<ReceiptDocument>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = _messages[MessageKeyEnum.UpdateFailed]
                };
            }
        }

        public async Task<OperationResult> DeleteReceiptDocument(int documentId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var document = await _dbContext.ReceiptDocuments
                    .Include(d => d.ReceiptResources)
                    .FirstOrDefaultAsync(d => d.Id == documentId);

                if (document != null)
                {
                    if (document.ReceiptResources.Any())
                    {
                        foreach (var resource in document.ReceiptResources)
                        {
                            var balance = await _dbContext.Balances
                                .FirstOrDefaultAsync(b => b.ResourceId == resource.ResourceId && b.UnitId == resource.UnitId);

                            if (balance == null || balance.Count < resource.Count)
                            {
                                return new OperationResult
                                {
                                    StatusCode = HttpStatusCode.BadRequest,
                                    Message = _messages[MessageKeyEnum.NotEnoughResource]
                                };
                            }
                            else
                            {
                                balance.Count -= resource.Count;
                            }
                        }
                        _dbContext.ReceiptResources.RemoveRange(document.ReceiptResources);
                    }
                    _dbContext.ReceiptDocuments.Remove(document);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.NoContent,
                        Message = _messages[MessageKeyEnum.Deleted]
                    };
                }
                else
                {
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = _messages[MessageKeyEnum.NotFound]
                    };
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, _messages[MessageKeyEnum.DeletionFailed]);
                return new OperationResult
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = _messages[MessageKeyEnum.DeletionFailed]
                };
            }
        }
        private async Task<int> GetDocumentNumber() =>
            (await _dbContext.ReceiptDocuments.MaxAsync(d => (int?)d.Number) ?? 0) + 1;
    }
}
