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

        public async Task<OperationResult<Unit>> CreateUnitAsync(Unit unit)
        {
            if (string.IsNullOrWhiteSpace(unit.Name))
            {
                return new OperationResult<Unit>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = _messages[MessageKeyEnum.NameRequired]
                };
            }
            var result = await _catalogService.CreateCatalogEntityAsync(unit, _dbContext.Units, DisplayedClassNames.Unit);
            result.Message = result.StatusCode switch
            {
                HttpStatusCode.Created => _messages[MessageKeyEnum.Created],
                HttpStatusCode.Conflict => _messages[MessageKeyEnum.AlreadyExists],
                HttpStatusCode.InternalServerError => _messages[MessageKeyEnum.CreationFailed],
                _ => string.Empty
            };
            return result;
        }

        public async Task<OperationResult<Unit>> UpdateUnitAsync(Unit unit)
        {
            if (string.IsNullOrWhiteSpace(unit.Name))
            {
                return new OperationResult<Unit>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = _messages[MessageKeyEnum.NameRequired]
                };
            }
            try
            {
                _dbContext.Units.Update(unit);
                await _dbContext.SaveChangesAsync();
                return new OperationResult<Unit>
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = _messages[MessageKeyEnum.Updated],
                    Data = unit
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _messages[MessageKeyEnum.UpdateFailed]);
                return new OperationResult<Unit>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = _messages[MessageKeyEnum.UpdateFailed],
                    Data = unit
                };
            }
        }

        public async Task<OperationResult> DeleteUnitAsync(int unitId)
        {
            try
            {
                var unit = await _dbContext.Units.FindAsync(unitId);
                var ReceiptResources = await _dbContext.ReceiptResources
                    .Where(r => r.UnitId == unitId)
                    .ToListAsync();
                var ShipmentResources = await _dbContext.ShipmentResources
                    .Where(r => r.UnitId == unitId)
                    .ToListAsync();
                if (ReceiptResources.Any() || ShipmentResources.Any())
                {
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.Conflict,
                        Message = _messages[MessageKeyEnum.InUse],
                    };
                }
                if (unit != null)
                {
                    _dbContext.Units.Remove(unit);
                    await _dbContext.SaveChangesAsync();
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.OK,
                        Message = _messages[MessageKeyEnum.Deleted],
                    };
                }
                else
                {
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = _messages[MessageKeyEnum.NotFound],
                    };
                }
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

        public async Task<OperationResult> ArchiveUnitAsync(Unit unit)
        {
            var result = await _catalogService.ArchiveCatalogEntityAsync(unit, _dbContext.Units);
            result.Message = result.StatusCode switch
            {
                HttpStatusCode.OK => _messages[MessageKeyEnum.Archived],
                HttpStatusCode.NotFound => _messages[MessageKeyEnum.NotFound],
                HttpStatusCode.InternalServerError => _messages[MessageKeyEnum.ArchiveFailed],
                _ => string.Empty
            };
            return result;
        }

        public async Task<OperationResult> ArchiveMultipleUnitAsync(Unit[] units) =>
            await _catalogService.ArchiveMultipleEntitiesAsync(units, _dbContext.Units, ArchiveUnitAsync);
    }
}
