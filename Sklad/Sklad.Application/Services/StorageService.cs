using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sklad.Contracts.Dtos;
using Sklad.Contracts.Requests;
using Sklad.Contracts.Responses;
using Sklad.Domain.Constants;
using Sklad.Domain.Enums;
using Sklad.Domain.Interfaces;
using Sklad.Domain.Models;
using Sklad.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace Sklad.Application.Services
{
    public class StorageService : IStorageService
    {
        private readonly SkladDbContext _dbContext;
        private readonly ILogger _logger;

        public StorageService(SkladDbContext dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<Balance>> GetStorageBalanceAsync(int? resourceId, int? unitId) =>
            await _dbContext.Balances
                .Where(b => (!resourceId.HasValue || b.ResourceId == resourceId) &&
                            (!unitId.HasValue || b.UnitOfMeasurementId == unitId))
                .Include(b => b.Resource)
                .Include(b => b.UnitOfMeasurement)
                .ToListAsync();

        public async Task<List<GoodsReceiptDocument>> GetGoodsReceiptDocumentsAsync(DocumentFilterDto filters)
        {
            var documents = _dbContext.GoodsReceiptDocuments
                .Include(d => d.InboundResources)
                .ThenInclude(r => r.Resource)
                .Include(d => d.InboundResources)
                .ThenInclude(r => r.UnitOfMeasurement)
                .AsQueryable();

            if (filters != null)
            {
                if (filters.DocumentNumbers?.Any() == true)
                    documents = documents.Where(d => filters.DocumentNumbers.Contains(d.Number));

                if (filters.ResourceIds?.Any() == true)
                    documents = documents.Where(d => d.InboundResources.Any(or => filters.ResourceIds.Contains(or.ResourceId)));

                if (filters.UnitOfMeasurementIds?.Any() == true)
                    documents = documents.Where(d => d.InboundResources.Any(or => filters.UnitOfMeasurementIds.Contains(or.UnitOfMeasurementId)));

                if (filters.FromDate.HasValue)
                    documents = documents.Where(d => d.Date >= filters.FromDate.Value);

                if (filters.ToDate.HasValue)
                    documents = documents.Where(d => d.Date <= filters.ToDate.Value);
            }
            return await documents.ToListAsync();
        }

        public async Task<OperationResult<GoodsReceiptDocument>> CreateGoodsReceiptDocumentAsync(CreateGoodsReceiptDocumentRequest request)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var document = new GoodsReceiptDocument
                {
                    Number = GetDocumentNumber(typeof(GoodsReceiptDocument)),
                    Date = DateTime.UtcNow,
                };

                await _dbContext.GoodsReceiptDocuments.AddAsync(document);

                if (request.Resources.Any())
                {
                    await _dbContext.InboundResources.AddRangeAsync(request.Resources.Select(r =>
                        new InboundResource
                        {
                            GoodsReceiptDocument = document,
                            ResourceId = r.ResourceId,
                            UnitOfMeasurementId = r.UnitOfMeasurementId,
                            Count = r.Count
                        }));

                    foreach (var resource in request.Resources)
                    {
                        var balance = await _dbContext.Balances
                            .FirstOrDefaultAsync(b => b.ResourceId == resource.ResourceId && b.UnitOfMeasurementId == resource.UnitOfMeasurementId);

                        if (balance == null)
                        {
                            balance = new Balance
                            {
                                ResourceId = resource.ResourceId,
                                UnitOfMeasurementId = resource.UnitOfMeasurementId,
                                Count = 0
                            };
                            await _dbContext.Balances.AddAsync(balance);
                        }

                        balance.Count += resource.Count;
                    }
                }

                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return new OperationResult<GoodsReceiptDocument>
                {
                    StatusCode = HttpStatusCode.Created,
                    Message = OperationResultMessages.GoodsReceiptDocumentCreated,
                    Data = document
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, OperationResultMessages.GoodsReceiptDocumentCreationFailed);
                return new OperationResult<GoodsReceiptDocument>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = OperationResultMessages.GoodsReceiptDocumentCreationFailed
                };
            }
        }

        public async Task<OperationResult<GoodsReceiptDocument>> UpdateGoodsReceiptDocumentAsync(UpdateGoodsReceiptDocumentRequest request)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var document = await _dbContext.GoodsReceiptDocuments
                    .Include(d => d.InboundResources)
                    .FirstOrDefaultAsync(d => d.Id == request.DocumentId);
                if (document == null)
                {
                    return new OperationResult<GoodsReceiptDocument>
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = OperationResultMessages.GoodsReceiptDocumentNotFound
                    };
                }
                if (request.Resources != null && request.Resources.Any())
                {
                    foreach (var resource in document.InboundResources.ToList())
                    {
                        var updatedResource = request.Resources.FirstOrDefault(r => r.ResourceId == resource.ResourceId && r.UnitOfMeasurementId == resource.UnitOfMeasurementId);
                        var balance = await _dbContext.Balances
                            .FirstOrDefaultAsync(b => b.ResourceId == resource.ResourceId && b.UnitOfMeasurementId == resource.UnitOfMeasurementId);
                        if (balance == null)
                        {
                            balance = new Balance
                            {
                                ResourceId = resource.ResourceId,
                                UnitOfMeasurementId = resource.UnitOfMeasurementId,
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
                                    return new OperationResult<GoodsReceiptDocument>
                                    {
                                        StatusCode = HttpStatusCode.BadRequest,
                                        Message = OperationResultMessages.NotEnoughResource
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
                                return new OperationResult<GoodsReceiptDocument>
                                {
                                    StatusCode = HttpStatusCode.BadRequest,
                                    Message = OperationResultMessages.NotEnoughResource
                                };
                            }
                            balance.Count -= resource.Count;
                            _dbContext.InboundResources.Remove(resource);
                        }
                    }
                    foreach (var newResource in request.Resources.Where(r => !document.InboundResources.Any(ir => ir.ResourceId == r.ResourceId && ir.UnitOfMeasurementId == r.UnitOfMeasurementId)))
                    {
                        var balance = await _dbContext.Balances
                            .FirstOrDefaultAsync(b => b.ResourceId == newResource.ResourceId && b.UnitOfMeasurementId == newResource.UnitOfMeasurementId);
                        if (balance == null)
                        {
                            _dbContext.Balances.Add(new Balance
                            {
                                ResourceId = newResource.ResourceId,
                                UnitOfMeasurementId = newResource.UnitOfMeasurementId,
                                Count = newResource.Count
                            });
                        }
                        _dbContext.InboundResources.Add(new InboundResource
                        {
                            GoodsReceiptDocumentId = document.Id,
                            ResourceId = newResource.ResourceId,
                            UnitOfMeasurementId = newResource.UnitOfMeasurementId,
                            Count = newResource.Count
                        });
                    }
                }
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OperationResult<GoodsReceiptDocument>
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = OperationResultMessages.GoodsReceiptDocumentUpdated,
                    Data = document
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, OperationResultMessages.GoodsReceiptDocumentUpdateFailed);
                return new OperationResult<GoodsReceiptDocument>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = OperationResultMessages.GoodsReceiptDocumentUpdateFailed
                };
            }
        }

        public async Task<OperationResult> DeleteGoodsReceiptDocument(int documentId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var document = await _dbContext.GoodsReceiptDocuments
                    .Include(d => d.InboundResources)
                    .FirstOrDefaultAsync(d => d.Id == documentId);

                if (document != null)
                {
                    if (document.InboundResources.Any())
                    {
                        foreach (var resource in document.InboundResources)
                        {
                            var balance = await _dbContext.Balances
                                .FirstOrDefaultAsync(b => b.ResourceId == resource.ResourceId && b.UnitOfMeasurementId == resource.UnitOfMeasurementId);

                            if (balance == null || balance.Count < resource.Count)
                            {
                                return new OperationResult
                                {
                                    StatusCode = HttpStatusCode.BadRequest,
                                    Message = OperationResultMessages.NotEnoughResource
                                };
                            }
                            else
                            {
                                balance.Count -= resource.Count;
                            }
                        }
                        _dbContext.InboundResources.RemoveRange(document.InboundResources);
                    }
                    _dbContext.GoodsReceiptDocuments.Remove(document);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.NoContent,
                        Message = OperationResultMessages.GoodsReceiptDocumentDeleted
                    };
                }
                else
                {
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = OperationResultMessages.GoodsReceiptDocumentNotFound
                    };
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, OperationResultMessages.GoodsReceiptDocumentDeletionFailed);
                return new OperationResult
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = OperationResultMessages.GoodsReceiptDocumentDeletionFailed
                };
            }
        }

        public async Task<List<GoodsIssueDocument>> GetGoodsIssueDocumentsAsync(DocumentFilterDto filters)
        {
            var documents = _dbContext.GoodsIssueDocuments
                .Include(d => d.OutboundResources)
                .ThenInclude(r => r.Resource)
                .Include(d => d.OutboundResources)
                .ThenInclude(r => r.UnitOfMeasurement)
                .AsQueryable();

            if (filters != null)
            {
                if (filters.DocumentNumbers?.Any() == true)
                    documents = documents.Where(d => filters.DocumentNumbers.Contains(d.Number));

                if (filters.ResourceIds?.Any() == true)
                    documents = documents.Where(d => d.OutboundResources.Any(or => filters.ResourceIds.Contains(or.ResourceId)));

                if (filters.UnitOfMeasurementIds?.Any() == true)
                    documents = documents.Where(d => d.OutboundResources.Any(or => filters.UnitOfMeasurementIds.Contains(or.UnitOfMeasurementId)));

                if (filters.FromDate.HasValue)
                    documents = documents.Where(d => d.Date >= filters.FromDate.Value);

                if (filters.ToDate.HasValue)
                    documents = documents.Where(d => d.Date <= filters.ToDate.Value);
            }
            return await documents.ToListAsync();
        }

        public async Task<OperationResult<GoodsIssueDocument>> CreateGoodsIssueDocumentAsync(CreateGoodsIssueDocumentRequest request)
        {
            if (request.Resources == null || !request.Resources.Any())
            {
                return new OperationResult<GoodsIssueDocument>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = OperationResultMessages.NoResourcesProvided
                };
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var document = new GoodsIssueDocument
                {
                    Number = GetDocumentNumber(typeof(GoodsIssueDocument)),
                    Date = DateTime.UtcNow,
                    ClientId = request.ClientId,
                    State = DocumentStateEnum.NotSigned,
                };
                await _dbContext.GoodsIssueDocuments.AddAsync(document);
                await _dbContext.SaveChangesAsync();
                await _dbContext.OutboundResources.AddRangeAsync(request.Resources.Select(r =>
                new OutboundResource
                {
                    GoodsIssueDocumentId = document.Id,
                    ResourceId = r.ResourceId,
                    UnitOfMeasurementId = r.UnitOfMeasurementId,
                    Count = r.Count
                }));
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OperationResult<GoodsIssueDocument>
                {
                    StatusCode = HttpStatusCode.Created,
                    Message = OperationResultMessages.GoodsIssueDocumentCreated,
                    Data = document
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка создания документа отгрузки");
                return new OperationResult<GoodsIssueDocument>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = OperationResultMessages.GoodsIssueDocumentCreationFailed
                };
            }
        }

        public async Task<OperationResult<GoodsIssueDocument>> UpdateGoodsIssueDocumentAsync(UpdateGoodsIssueDocumentRequest request)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var document = await _dbContext.GoodsIssueDocuments
                    .Include(d => d.OutboundResources)
                    .FirstOrDefaultAsync(d => d.Id == request.DocumentId);
                if (document == null)
                {
                    return new OperationResult<GoodsIssueDocument>
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = OperationResultMessages.GoodsIssueDocumentNotFound
                    };
                }
                if (request.Resources != null && request.Resources.Any())
                {
                    foreach (var resource in document.OutboundResources.ToList())
                    {
                        var updatedResource = request.Resources.FirstOrDefault(r => r.ResourceId == resource.ResourceId && r.UnitOfMeasurementId == resource.UnitOfMeasurementId);
                        if (updatedResource != null)
                        {
                            resource.Count = updatedResource.Count;
                        }
                        else
                        {
                            _dbContext.OutboundResources.Remove(resource);
                        }
                    }
                    foreach (var newResource in request.Resources.Where(r => !document.OutboundResources.Any(or => or.ResourceId == r.ResourceId && or.UnitOfMeasurementId == r.UnitOfMeasurementId)))
                    {
                        _dbContext.OutboundResources.Add(new OutboundResource
                        {
                            GoodsIssueDocumentId = document.Id,
                            ResourceId = newResource.ResourceId,
                            UnitOfMeasurementId = newResource.UnitOfMeasurementId,
                            Count = newResource.Count
                        });
                    }
                }
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OperationResult<GoodsIssueDocument>
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = OperationResultMessages.GoodsIssueDocumentUpdated,
                    Data = document
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, OperationResultMessages.GoodsIssueDocumentUpdateFailed);
                return new OperationResult<GoodsIssueDocument>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = OperationResultMessages.GoodsIssueDocumentUpdateFailed
                };
            }
        }

        public async Task<OperationResult> DeleteGoodsIssueDocumentAsync(int documentId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var document = await _dbContext.GoodsIssueDocuments
                    .Include(d => d.OutboundResources)
                    .FirstOrDefaultAsync(d => d.Id == documentId);
                if (document == null)
                {
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = OperationResultMessages.GoodsIssueDocumentNotFound
                    };
                }
                var outboundResources = document.OutboundResources;
                if (outboundResources.Any())
                {
                    _dbContext.OutboundResources.RemoveRange(outboundResources);
                }
                _dbContext.GoodsIssueDocuments.Remove(document);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OperationResult
                {
                    StatusCode = HttpStatusCode.NoContent,
                    Message = OperationResultMessages.GoodsIssueDocumentDeleted
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, OperationResultMessages.GoodsIssueDocumentDeletionFailed);
                return new OperationResult
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = OperationResultMessages.GoodsIssueDocumentDeletionFailed
                };
            }
        }

        public async Task<OperationResult<GoodsIssueDocument>> SignGoodsIssueDocumentAsync(int documentId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var document = await _dbContext.GoodsIssueDocuments
                    .Include(d => d.OutboundResources)
                    .FirstOrDefaultAsync(d => d.Id == documentId);
                if (document == null)
                {
                    return new OperationResult<GoodsIssueDocument>
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = OperationResultMessages.GoodsIssueDocumentNotFound
                    };
                }
                if (document.State != DocumentStateEnum.Signed)
                {
                    return new OperationResult<GoodsIssueDocument>
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = OperationResultMessages.GoodsIssueDocumentAlreadySigned
                    };
                }
                foreach (var resource in document.OutboundResources)
                {
                    var balance = await _dbContext.Balances
                        .FirstOrDefaultAsync(b => b.ResourceId == resource.ResourceId && b.UnitOfMeasurementId == resource.UnitOfMeasurementId);
                    if (balance == null || balance.Count < resource.Count)
                    {
                        return new OperationResult<GoodsIssueDocument>
                        {
                            StatusCode = HttpStatusCode.BadRequest,
                            Message = OperationResultMessages.NotEnoughResource
                        };
                    }
                    balance.Count -= resource.Count;
                }
                document.State = DocumentStateEnum.Signed;
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OperationResult<GoodsIssueDocument>
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = OperationResultMessages.GoodsIssueDocumentSigned,
                    Data = document
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, OperationResultMessages.GoodsIssueDocumentSigningFailed);
                return new OperationResult<GoodsIssueDocument>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = OperationResultMessages.GoodsIssueDocumentSigningFailed
                };
            }
        }

        public async Task<OperationResult<GoodsIssueDocument>> WithdrawGoodsIssueDocumentAsync(int documentId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var document = await _dbContext.GoodsIssueDocuments
                    .Include(d => d.OutboundResources)
                    .FirstOrDefaultAsync(d => d.Id == documentId);
                if (document == null)
                {
                    return new OperationResult<GoodsIssueDocument>
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = OperationResultMessages.GoodsIssueDocumentNotFound
                    };
                }
                if (document.State == DocumentStateEnum.Withdrawn)
                {
                    return new OperationResult<GoodsIssueDocument>
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = OperationResultMessages.GoodsIssueDocumentAlreadyWithdrawn
                    };
                }
                foreach (var resource in document.OutboundResources)
                {
                    var balance = await _dbContext.Balances
                        .FirstOrDefaultAsync(b => b.ResourceId == resource.ResourceId && b.UnitOfMeasurementId == resource.UnitOfMeasurementId);
                    if (balance != null)
                    {
                        balance.Count += resource.Count;
                    }
                }
                document.State = DocumentStateEnum.Withdrawn;
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OperationResult<GoodsIssueDocument>
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = OperationResultMessages.GoodsIssueDocumentWithdrawn,
                    Data = document
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка подписания документа отгрузки");
                return new OperationResult<GoodsIssueDocument>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = OperationResultMessages.GoodsIssueDocumentWithdrawalFailed
                };
            }
        }

        private int GetDocumentNumber(Type type) =>
            type switch
            {
                Type t when t == typeof(GoodsReceiptDocument) =>
                    (_dbContext.GoodsReceiptDocuments
                    .OrderByDescending(d => d.Number)
                    .FirstOrDefault()?.Number ?? 0) + 1,
                Type t when t == typeof(GoodsIssueDocument) =>
                    (_dbContext.GoodsIssueDocuments
                    .OrderByDescending(d => d.Number)
                    .FirstOrDefault()?.Number ?? 0) + 1,
                _ => throw new InvalidOperationException("Неизвестный тип документа")
            };
    }
}