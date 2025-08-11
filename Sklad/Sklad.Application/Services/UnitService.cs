using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sklad.Application.Interfaces;
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
    public class UnitService : IUnitService
    {
        private readonly SkladDbContext _dbContext;
        private readonly ILogger<UnitService> _logger;
        private readonly ICatalogService _catalogService;
        private readonly IReadOnlyDictionary<MessageKeyEnum, string> _messages;
        public UnitService(SkladDbContext dbContext, ILogger<UnitService> logger, ICatalogService catalogService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _catalogService = catalogService;
            _messages = OperationResultMessages.GetMessagesFor(typeof(Unit));
        }

        public async Task<List<Unit>> GetUnitsAsync(CatalogEntityStateEnum? state) =>
            await _dbContext.Units.Where(r => !state.HasValue || r.State == state.Value).ToListAsync();

        public async Task<OperationResult<Unit>> CreateUnitAsync(Unit unit) => 
            await _catalogService.CreateCatalogEntityAsync(unit, _dbContext.Units);

        public async Task<OperationResult<Unit>> UpdateUnitAsync(Unit unit) =>
            await _catalogService.UpdateCatalogEntityAsync(unit, _dbContext.Units);

        public async Task<OperationResult> DeleteUnitAsync(int unitId)
        {
            try
            {
                var unit = await _dbContext.Units.FindAsync(unitId);
                if (unit == null)
                {
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = _messages[MessageKeyEnum.NotFound]
                    };
                }
                var receiptResources = await _dbContext.ReceiptResources
                    .Where(r => r.UnitId == unitId)
                    .ToListAsync();
                var shipmentResources = await _dbContext.ShipmentResources
                    .Where(r => r.UnitId == unitId)
                    .ToListAsync();
                if (receiptResources.Any() || shipmentResources.Any())
                {
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.Conflict,
                        Message = _messages[MessageKeyEnum.InUse],
                    };
                }
                return await _catalogService.DeleteCatalogEntityAsync(unit, _dbContext.Units);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при удалении единицы измерения с ID {unitId}");
                return new OperationResult
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = _messages[MessageKeyEnum.DeletionFailed],
                };
            }
        }

        public async Task<OperationResult> DeleteMultipleUnitsAsync(int[] unitIds)
        {
            var units = await _dbContext.Units
                .Where(u => unitIds.Contains(u.Id))
                .ToListAsync();
            var notFoundCount = unitIds.Length - units.Count;
            var inUseUnits = units
                .Where(u =>
                    _dbContext.ReceiptResources.Any(r => r.UnitId == u.Id) ||
                    _dbContext.ShipmentResources.Any(r => r.UnitId == u.Id))
                .ToList();
            return await _catalogService.DeleteMultipleEntitiesAsync(_dbContext.Units, inUseUnits, units, notFoundCount);
        }


        public async Task<OperationResult> ArchiveUnitAsync(Unit unit) => 
            await _catalogService.ArchiveCatalogEntityAsync(unit, _dbContext.Units);

        public async Task<OperationResult> ArchiveMultipleUnitsAsync(Unit[] units) =>
            await _catalogService.ArchiveMultipleEntitiesAsync(units, _dbContext.Units);
    }
}
