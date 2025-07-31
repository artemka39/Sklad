using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sklad.Domain.Constants;
using Sklad.Domain.Interfaces;
using Sklad.Domain.Models;
using Sklad.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<List<GoodsReceiptDocument>> GetGoodsReceiptDocumentsAsync() =>
            await _dbContext.GoodsReceiptDocuments
                .Include(d => d.InboundResources)
                .ThenInclude(r => r.Resource)
                .Include(d => d.InboundResources)
                .ThenInclude(r => r.UnitOfMeasurement)
                .ToListAsync();

        public async Task CreateGoodsReceiptDocumentAsync(GoodsReceiptDocument document, List<InboundResource> resources)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                if (resources.Any())
                {
                    foreach (var resource in resources)
                    {
                        var balance = await _dbContext.Balances
                            .FirstOrDefaultAsync(b => b.ResourceId == resource.ResourceId && b.UnitOfMeasurementId == resource.UnitOfMeasurementId);
                        if (balance != null)
                        {
                            balance.Count += resource.Count;
                        }
                    }
                    await _dbContext.InboundResources.AddRangeAsync(resources);
                }
                await _dbContext.GoodsReceiptDocuments.AddAsync(document);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка создания документа поступления");
            }
        }

        public async Task DeleteGoodsReceiptDocument(Guid documentId)
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

        public async Task CreateGoodsIssueDocumentAsync(GoodsIssueDocument document, List<OutboundResource> resources)
        {
            if (resources == null || !resources.Any())
            {
                throw new InvalidOperationException("Документ отгрузки не может быть пустым.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await _dbContext.OutboundResources.AddRangeAsync(resources);
                await _dbContext.GoodsIssueDocuments.AddAsync(document);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Ошибка создания документа отгрузки");
            }
        }
    }
}