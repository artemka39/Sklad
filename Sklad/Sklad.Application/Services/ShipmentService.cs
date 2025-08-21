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
    public class ShipmentService : IShipmentService
    {
        private readonly SkladDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IReadOnlyDictionary<MessageKeyEnum, string> _messages;
        private static readonly object _locker = new object();

        public ShipmentService(SkladDbContext dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
            _messages = OperationResultMessages.GetMessagesFor(typeof(ShipmentDocument));
        }
        public async Task<List<ShipmentDocument>> GetShipmentDocumentsAsync(DocumentFilterDto filters)
        {
            var documents = _dbContext.ShipmentDocuments
                .Include(d => d.Client)
                .Include(d => d.ShipmentResources)
                .ThenInclude(r => r.Resource)
                .Include(d => d.ShipmentResources)
                .ThenInclude(r => r.Unit)
                .AsQueryable();

            if (filters != null)
            {
                if (filters.DocumentNumbers?.Any() == true)
                    documents = documents.Where(d => filters.DocumentNumbers.Contains(d.Number));

                if (filters.ResourceIds?.Any() == true)
                    documents = documents.Where(d => d.ShipmentResources.Any(or => filters.ResourceIds.Contains(or.ResourceId)));

                if (filters.UnitIds?.Any() == true)
                    documents = documents.Where(d => d.ShipmentResources.Any(or => filters.UnitIds.Contains(or.UnitId)));

                if (filters.FromDate.HasValue)
                    documents = documents.Where(d => d.Date >= filters.FromDate.Value);

                if (filters.ToDate.HasValue)
                    documents = documents.Where(d => d.Date <= filters.ToDate.Value);
            }
            return await documents.ToListAsync();
        }

        public async Task<OperationResult<ShipmentDocument>> CreateShipmentDocumentAsync(CreateShipmentDocumentRequest request)
        {
            if (request.Resources == null || !request.Resources.Any())
            {
                return new OperationResult<ShipmentDocument>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = _messages[MessageKeyEnum.NoResourcesProvided]
                };
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var document = new ShipmentDocument
                {
                    Number = await GetDocumentNumber(),
                    Date = DateTime.UtcNow,
                    ClientId = request.ClientId,
                    State = DocumentStateEnum.NotSigned,
                };
                await _dbContext.ShipmentDocuments.AddAsync(document);
                await _dbContext.SaveChangesAsync();
                await _dbContext.ShipmentResources.AddRangeAsync(request.Resources.Select(r =>
                new ShipmentResource
                {
                    ShipmentDocumentId = document.Id,
                    ResourceId = r.ResourceId,
                    UnitId = r.UnitId,
                    Count = r.Count
                }));
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OperationResult<ShipmentDocument>
                {
                    StatusCode = HttpStatusCode.Created,
                    Message = _messages[MessageKeyEnum.Created],
                    Data = document
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка создания документа отгрузки");
                return new OperationResult<ShipmentDocument>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = _messages[MessageKeyEnum.CreationFailed]
                };
            }
        }

        public async Task<OperationResult<ShipmentDocument>> UpdateShipmentDocumentAsync(UpdateShipmentDocumentRequest request)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var document = await _dbContext.ShipmentDocuments
                    .Include(d => d.ShipmentResources)
                    .FirstOrDefaultAsync(d => d.Id == request.DocumentId);
                if (document == null)
                {
                    return new OperationResult<ShipmentDocument>
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = _messages[MessageKeyEnum.NotFound]
                    };
                }
                if (request.Resources != null && request.Resources.Any())
                {
                    foreach (var resource in document.ShipmentResources.ToList())
                    {
                        var updatedResource = request.Resources.FirstOrDefault(r => r.ResourceId == resource.ResourceId && r.UnitId == resource.UnitId);
                        if (updatedResource != null)
                        {
                            resource.Count = updatedResource.Count;
                        }
                        else
                        {
                            _dbContext.ShipmentResources.Remove(resource);
                        }
                    }
                    foreach (var newResource in request.Resources.Where(r => !document.ShipmentResources.Any(or => or.ResourceId == r.ResourceId && or.UnitId == r.UnitId)))
                    {
                        _dbContext.ShipmentResources.Add(new ShipmentResource
                        {
                            ShipmentDocumentId = document.Id,
                            ResourceId = newResource.ResourceId,
                            UnitId = newResource.UnitId,
                            Count = newResource.Count
                        });
                    }
                }
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OperationResult<ShipmentDocument>
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
                return new OperationResult<ShipmentDocument>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = _messages[MessageKeyEnum.UpdateFailed]
                };
            }
        }

        public async Task<OperationResult> DeleteShipmentDocumentAsync(int documentId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var document = await _dbContext.ShipmentDocuments
                    .Include(d => d.ShipmentResources)
                    .FirstOrDefaultAsync(d => d.Id == documentId);
                if (document == null)
                {
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = _messages[MessageKeyEnum.NotFound]
                    };
                }
                var ShipmentResources = document.ShipmentResources;
                if (ShipmentResources.Any())
                {
                    _dbContext.ShipmentResources.RemoveRange(ShipmentResources);
                }
                _dbContext.ShipmentDocuments.Remove(document);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OperationResult
                {
                    StatusCode = HttpStatusCode.NoContent,
                    Message = _messages[MessageKeyEnum.Deleted]
                };
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

        public async Task<OperationResult<ShipmentDocument>> SignShipmentDocumentAsync(int documentId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var document = await _dbContext.ShipmentDocuments
                    .Include(d => d.ShipmentResources)
                    .FirstOrDefaultAsync(d => d.Id == documentId);
                if (document == null)
                {
                    return new OperationResult<ShipmentDocument>
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = _messages[MessageKeyEnum.NotFound]
                    };
                }
                if (document.State == DocumentStateEnum.Signed)
                {
                    return new OperationResult<ShipmentDocument>
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = _messages[MessageKeyEnum.AlreadySigned]
                    };
                }
                foreach (var resource in document.ShipmentResources)
                {
                    var balance = await _dbContext.Balances
                        .FirstOrDefaultAsync(b => b.ResourceId == resource.ResourceId && b.UnitId == resource.UnitId);
                    if (balance == null || balance.Count < resource.Count)
                    {
                        return new OperationResult<ShipmentDocument>
                        {
                            StatusCode = HttpStatusCode.BadRequest,
                            Message = _messages[MessageKeyEnum.NotEnoughResource]
                        };
                    }
                    balance.Count -= resource.Count;
                }
                document.State = DocumentStateEnum.Signed;
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OperationResult<ShipmentDocument>
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = _messages[MessageKeyEnum.Signed],
                    Data = document
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, _messages[MessageKeyEnum.SigningFailed]);
                return new OperationResult<ShipmentDocument>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = _messages[MessageKeyEnum.SigningFailed]
                };
            }
        }

        public async Task<OperationResult<ShipmentDocument>> WithdrawShipmentDocumentAsync(int documentId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var document = await _dbContext.ShipmentDocuments
                    .Include(d => d.ShipmentResources)
                    .FirstOrDefaultAsync(d => d.Id == documentId);
                if (document == null)
                {
                    return new OperationResult<ShipmentDocument>
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = _messages[MessageKeyEnum.NotFound]
                    };
                }
                if (document.State == DocumentStateEnum.NotSigned)
                {
                    return new OperationResult<ShipmentDocument>
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = _messages[MessageKeyEnum.AlreadyWithdrawn]
                    };
                }
                foreach (var resource in document.ShipmentResources)
                {
                    var balance = await _dbContext.Balances
                        .FirstOrDefaultAsync(b => b.ResourceId == resource.ResourceId && b.UnitId == resource.UnitId);
                    if (balance != null)
                    {
                        balance.Count += resource.Count;
                    }
                }
                document.State = DocumentStateEnum.NotSigned;
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OperationResult<ShipmentDocument>
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = _messages[MessageKeyEnum.Withdrawn],
                    Data = document
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка подписания документа отгрузки");
                return new OperationResult<ShipmentDocument>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = _messages[MessageKeyEnum.WithdrawalFailed]
                };
            }
        }

        private async Task<int> GetDocumentNumber() =>
            (await _dbContext.ShipmentDocuments.MaxAsync(d => (int?)d.Number) ?? 0) + 1;
    }
}
