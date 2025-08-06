using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Logging;
using Sklad.Domain.Constants;
using Sklad.Domain.Enums;
using Sklad.Domain.Interfaces;
using Sklad.Domain.Models;
using Sklad.Persistence;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sklad.Application.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly SkladDbContext _dbContext;
        private readonly ILogger _logger;

        public CatalogService(SkladDbContext dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<Resource>> GetResourcesAsync() =>
            await _dbContext.Resources.ToListAsync();

        public async Task CreateResourceAsync(Resource resource)
        {
            await CreateCatalogEntityAsync(resource, _dbContext.Resources, DisplayedClassNames.Resource);
        }

        public async Task UpdateResourceAsync(Resource resource)
        {
            try
            {
                _dbContext.Resources.Update(resource);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при обновлении ресурса с ID {resource.Id}");
                throw;
            }
        }

        public async Task DeleteResourceAsync(int resourceId)
        {
            try
            {
                var resource = await _dbContext.Resources.FindAsync(resourceId);
                var inboundResources = await _dbContext.InboundResources
                    .Where(r => r.ResourceId == resourceId)
                    .ToListAsync();
                var outboundResources = await _dbContext.OutboundResources
                    .Where(r => r.ResourceId == resourceId)
                    .ToListAsync();
                if (inboundResources.Any() || outboundResources.Any())
                {
                    throw new InvalidOperationException($"Невозможно удалить ресурс с ID {resourceId}, так как он используется в документах");
                }
                if (resource != null)
                {
                    _dbContext.Resources.Remove(resource);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    throw new InvalidOperationException($"Ресурс с ID {resourceId} не найден");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при удалении ресурса с ID {resourceId}");
                throw;
            }
        }

        public async Task ArchiveResource(Resource resource)
        {
            await ArchiveCatalogEntityAsync(resource, _dbContext.Resources);
        }

        public async Task<List<UnitOfMeasurement>> GetUnitOfMeasurementsAsync() =>
            await _dbContext.UnitOfMeasurements.ToListAsync();

        public async Task CreateUnitOfMeasurementAsync(UnitOfMeasurement unitOfMeasurement)
        {
            await CreateCatalogEntityAsync(unitOfMeasurement, _dbContext.UnitOfMeasurements, DisplayedClassNames.UnitOfMeasurement);
        }

        public async Task UpdateUnitOfMeasurementAsync(UnitOfMeasurement unitOfMeasurement)
        {
            try
            {
                _dbContext.UnitOfMeasurements.Update(unitOfMeasurement);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при обновлении единицы измерения с ID {unitOfMeasurement.Id}");
                throw;
            }
        }

        public async Task DeleteUnitOfMeasurementAsync(int unitOfMeasurementId)
        {
            try
            {
                var unit = await _dbContext.UnitOfMeasurements.FindAsync(unitOfMeasurementId);
                var inboundResources = await _dbContext.InboundResources
                    .Where(r => r.UnitOfMeasurementId == unitOfMeasurementId)
                    .ToListAsync();
                var outboundResources = await _dbContext.OutboundResources
                    .Where(r => r.UnitOfMeasurementId == unitOfMeasurementId)
                    .ToListAsync();
                if (inboundResources.Any() || outboundResources.Any())
                {
                    throw new InvalidOperationException(
                        $"Невозможно удалить единицу измерения с ID {unitOfMeasurementId}, так как она используется в ресурсах");
                }
                if (unit != null)
                {
                    _dbContext.UnitOfMeasurements.Remove(unit);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    throw new InvalidOperationException($"Единица измерения с ID {unitOfMeasurementId} не найдена");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при удалении единицы измерения с ID {unitOfMeasurementId}");
                throw;
            }
        }

        public async Task ArchiveUnitOfMeasurementAsync(UnitOfMeasurement unit)
        {
            await ArchiveCatalogEntityAsync(unit, _dbContext.UnitOfMeasurements);
        }

        public async Task<List<Client>> GetClientsAsync() =>
            await _dbContext.Clients.ToListAsync();

        public async Task CreateClientAsync(Client client)
        {
            await CreateCatalogEntityAsync(client, _dbContext.Clients, DisplayedClassNames.Client);
        }

        public async Task UpdateClientAsync(Client client)
        {
            try
            {
                _dbContext.Clients.Update(client);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при обновлении клиента с ID {client.Id}");
                throw;
            }
        }

        public async Task DeleteClientAsync(int clientId)
        {
            try
            {
                var client = await _dbContext.Clients.FindAsync(clientId);
                var goodsIssueDocuments = await _dbContext.GoodsIssueDocuments
                    .Where(d => d.ClientId == clientId)
                    .ToListAsync();
                if (goodsIssueDocuments.Any())
                {
                    throw new InvalidOperationException(
                        $"Невозможно удалить клиента с ID {clientId}, так как он используется в документах отгрузки");
                }
                if (client != null)
                {
                    _dbContext.Clients.Remove(client);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    throw new InvalidOperationException($"Клиент с ID {clientId} не найден");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при удалении клиента с ID {clientId}");
                throw;
            }
        }

        public async Task ArchiveClientAsync(Client client)
        {
            await ArchiveCatalogEntityAsync(client, _dbContext.Clients);
        }

        private async Task CreateCatalogEntityAsync<TEntity>(
            TEntity entity,
            DbSet<TEntity> dbSet,
            string entityDisplayName)
            where TEntity : class, ICatalogEntity
        {
            try
            {
                var existingEntity = await dbSet.FirstOrDefaultAsync(e => e.Name == entity.Name);
                if (existingEntity == null)
                {
                    entity.State = CatalogEntityStateEnum.Active;
                    await dbSet.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    throw new InvalidOperationException($"{entityDisplayName} с наименованием {entity.Name} уже существует");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при создании {entityDisplayName.ToLower()}");
                throw;
            }
        }

        private async Task ArchiveCatalogEntityAsync<TEntity>(
            TEntity entity,
            DbSet<TEntity> dbSet)
            where TEntity : class, ICatalogEntity
        {
            try
            {
                var existingEntity = await dbSet.FirstOrDefaultAsync(e => e.Name == entity.Name);
                if (existingEntity != null)
                {
                    existingEntity.State = CatalogEntityStateEnum.Archived;
                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
