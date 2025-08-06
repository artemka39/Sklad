using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sklad.Contracts.Requests;
using Sklad.Domain.Constants;
using Sklad.Domain.Interfaces;
using Sklad.Domain.Models;
using Sklad.Domain.Enums;
using Sklad.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<List<GoodsReceiptDocument>> GetGoodsReceiptDocumentsAsync() =>
            await _dbContext.GoodsReceiptDocuments
                .Include(d => d.InboundResources)
                .ThenInclude(r => r.Resource)
                .Include(d => d.InboundResources)
                .ThenInclude(r => r.UnitOfMeasurement)
                .ToListAsync();

        public async Task CreateGoodsReceiptDocumentAsync(CreateGoodsReceiptDocumentRequest request)
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
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка создания документа поступления");
            }
        }

        public async Task DeleteGoodsReceiptDocument(int documentId)
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
                                throw new InvalidOperationException(
                                    $"Недостаточно остатка для ресурса (ID: {resource.ResourceId}, Ед. изм.: {resource.UnitOfMeasurementId}). Удаление невозможно.");
                            }
                        }

                        foreach (var resource in document.InboundResources)
                        {
                            var balance = await _dbContext.Balances
                                .FirstOrDefaultAsync(b => b.ResourceId == resource.ResourceId && b.UnitOfMeasurementId == resource.UnitOfMeasurementId);

                            balance.Count -= resource.Count;
                        }

                        _dbContext.InboundResources.RemoveRange(document.InboundResources);
                    }

                    _dbContext.GoodsReceiptDocuments.Remove(document);
                    await _dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка удаления доумента поступления");
            }
        }

        public async Task<List<GoodsIssueDocument>> GetGoodsIssueDocumentsAsync() =>
            await _dbContext.GoodsIssueDocuments
                .Include(d => d.OutboundResources)
                .ThenInclude(r => r.Resource)
                .Include(d => d.OutboundResources)
                .ThenInclude(r => r.UnitOfMeasurement)
                .ToListAsync();

        public async Task CreateGoodsIssueDocumentAsync(CreateGoodsIssueDocumentRequest request)
        {
            if (request.Resources == null || !request.Resources.Any())
            {
                throw new InvalidOperationException("Документ отгрузки не может быть пустым.");
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
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка создания документа отгрузки");
            }
        }

        public async Task SignGoodsIssueDocumentAsync(int documentId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var document = await _dbContext.GoodsIssueDocuments
                    .Include(d => d.OutboundResources)
                    .FirstOrDefaultAsync(d => d.Id == documentId);
                if (document == null)
                {
                    throw new InvalidOperationException("Документ не найден.");
                }
                if (document.State != DocumentStateEnum.Signed)
                {
                    throw new InvalidOperationException("Документ уже подписан.");
                }
                foreach (var resource in document.OutboundResources)
                {
                    var balance = await _dbContext.Balances
                        .FirstOrDefaultAsync(b => b.ResourceId == resource.ResourceId && b.UnitOfMeasurementId == resource.UnitOfMeasurementId);
                    if (balance == null || balance.Count < resource.Count)
                    {
                        throw new InvalidOperationException(
                            $"Недостаточно остатка для ресурса (ID: {resource.ResourceId}, Ед. изм.: {resource.UnitOfMeasurementId}). Подписание невозможно.");
                    }
                    balance.Count -= resource.Count;
                }
                document.State = DocumentStateEnum.Signed;
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка подписания документа отгрузки");
            }
        }

        public async Task WithdrawGoodsIssueDocumentAsync(int documentId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var document = await _dbContext.GoodsIssueDocuments
                    .Include(d => d.OutboundResources)
                    .FirstOrDefaultAsync(d => d.Id == documentId);
                if (document == null)
                {
                    throw new InvalidOperationException("Документ не найден.");
                }
                if (document.State != DocumentStateEnum.Withdrawn)
                {
                    throw new InvalidOperationException("Документ уже отозван.");
                }
                foreach (var resource in document.OutboundResources)
                {
                    var balance = await _dbContext.Balances
                        .FirstOrDefaultAsync(b => b.ResourceId == resource.ResourceId && b.UnitOfMeasurementId == resource.UnitOfMeasurementId);
                    if (balance == null || balance.Count < resource.Count)
                    {
                        throw new InvalidOperationException();
                    }
                    balance.Count += resource.Count;
                }
                document.State = DocumentStateEnum.Withdrawn;
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка подписания документа отгрузки");
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