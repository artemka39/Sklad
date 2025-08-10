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
        private readonly IReadOnlyDictionary<MessageKeyEnum, string> _receiptDocumentMessages;
        private readonly IReadOnlyDictionary<MessageKeyEnum, string> _shipmentDocumentMessages;

        public StorageService(SkladDbContext dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
            _receiptDocumentMessages = OperationResultMessages.GetMessagesFor(typeof(ReceiptDocument));
            _shipmentDocumentMessages = OperationResultMessages.GetMessagesFor(typeof(ShipmentDocument));
        }

        public async Task<List<Balance>> GetStorageBalanceAsync(int? resourceId, int? unitId) =>
            await _dbContext.Balances
                .Where(b => (!resourceId.HasValue || b.ResourceId == resourceId) &&
                            (!unitId.HasValue || b.UnitId == unitId))
                .Include(b => b.Resource)
                .Include(b => b.Unit)
                .ToListAsync();

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
                    documents = documents.Where(d => filters.DocumentNumbers.Contains(d.Number));

                if (filters.ResourceIds?.Any() == true)
                    documents = documents.Where(d => d.ReceiptResources.Any(or => filters.ResourceIds.Contains(or.ResourceId)));

                if (filters.UnitIds?.Any() == true)
                    documents = documents.Where(d => d.ReceiptResources.Any(or => filters.UnitIds.Contains(or.UnitId)));

                if (filters.FromDate.HasValue)
                    documents = documents.Where(d => d.Date >= filters.FromDate.Value);

                if (filters.ToDate.HasValue)
                    documents = documents.Where(d => d.Date <= filters.ToDate.Value);
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
                    Number = GetDocumentNumber(typeof(ReceiptDocument)),
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
                    Message = _receiptDocumentMessages[MessageKeyEnum.Created],
                    Data = document
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, _receiptDocumentMessages[MessageKeyEnum.CreationFailed]);
                return new OperationResult<ReceiptDocument>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = _receiptDocumentMessages[MessageKeyEnum.CreationFailed]
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
                        Message = _receiptDocumentMessages[MessageKeyEnum.NotFound]
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
                                        Message = _receiptDocumentMessages[MessageKeyEnum.NotEnoughResource]
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
                                    Message = _receiptDocumentMessages[MessageKeyEnum.NotEnoughResource]
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
                    Message = _receiptDocumentMessages[MessageKeyEnum.Updated],
                    Data = document
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, _receiptDocumentMessages[MessageKeyEnum.UpdateFailed]);
                return new OperationResult<ReceiptDocument>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = _receiptDocumentMessages[MessageKeyEnum.UpdateFailed]
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
                                    Message = _receiptDocumentMessages[MessageKeyEnum.NotEnoughResource]
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
                        Message = _receiptDocumentMessages[MessageKeyEnum.Deleted]
                    };
                }
                else
                {
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = _receiptDocumentMessages[MessageKeyEnum.NotFound]
                    };
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, _receiptDocumentMessages[MessageKeyEnum.DeletionFailed]);
                return new OperationResult
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = _receiptDocumentMessages[MessageKeyEnum.DeletionFailed]
                };
            }
        }

        public async Task<List<ShipmentDocument>> GetShipmentDocumentsAsync(DocumentFilterDto filters)
        {
            var documents = _dbContext.ShipmentDocuments
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
                    Message = _receiptDocumentMessages[MessageKeyEnum.NoResourcesProvided]  
                };
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var document = new ShipmentDocument
                {
                    Number = GetDocumentNumber(typeof(ShipmentDocument)),
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
                    Message = _shipmentDocumentMessages[MessageKeyEnum.Created],
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
                    Message = _shipmentDocumentMessages[MessageKeyEnum.CreationFailed]
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
                        Message = _shipmentDocumentMessages[MessageKeyEnum.NotFound]
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
                    Message = _shipmentDocumentMessages[MessageKeyEnum.Updated],
                    Data = document
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, _shipmentDocumentMessages[MessageKeyEnum.UpdateFailed]);
                return new OperationResult<ShipmentDocument>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = _shipmentDocumentMessages[MessageKeyEnum.UpdateFailed]
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
                        Message = _shipmentDocumentMessages[MessageKeyEnum.NotFound]
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
                    Message = _shipmentDocumentMessages[MessageKeyEnum.Deleted]
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, _shipmentDocumentMessages[MessageKeyEnum.DeletionFailed]);
                return new OperationResult
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = _shipmentDocumentMessages[MessageKeyEnum.DeletionFailed]
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
                        Message = _shipmentDocumentMessages[MessageKeyEnum.NotFound]
                    };
                }
                if (document.State != DocumentStateEnum.Signed)
                {
                    return new OperationResult<ShipmentDocument>
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = _shipmentDocumentMessages[MessageKeyEnum.AlreadySigned]
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
                            Message = _receiptDocumentMessages[MessageKeyEnum.NotEnoughResource]
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
                    Message = _shipmentDocumentMessages[MessageKeyEnum.Signed],
                    Data = document
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, _shipmentDocumentMessages[MessageKeyEnum.SigningFailed]);
                return new OperationResult<ShipmentDocument>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = _shipmentDocumentMessages[MessageKeyEnum.SigningFailed]
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
                        Message = _shipmentDocumentMessages[MessageKeyEnum.NotFound]
                    };
                }
                if (document.State == DocumentStateEnum.Withdrawn)
                {
                    return new OperationResult<ShipmentDocument>
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = _shipmentDocumentMessages[MessageKeyEnum.AlreadyWithdrawn]
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
                document.State = DocumentStateEnum.Withdrawn;
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return new OperationResult<ShipmentDocument>
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = _shipmentDocumentMessages[MessageKeyEnum.Withdrawn],
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
                    Message = _shipmentDocumentMessages[MessageKeyEnum.WithdrawalFailed]
                };
            }
        }

        private int GetDocumentNumber(Type type) =>
            type switch
            {
                Type t when t == typeof(ReceiptDocument) =>
                    (_dbContext.ReceiptDocuments
                    .OrderByDescending(d => d.Number)
                    .FirstOrDefault()?.Number ?? 0) + 1,
                Type t when t == typeof(ShipmentDocument) =>
                    (_dbContext.ShipmentDocuments
                    .OrderByDescending(d => d.Number)
                    .FirstOrDefault()?.Number ?? 0) + 1,
                _ => throw new InvalidOperationException("Неизвестный тип документа")
            };
    }
}