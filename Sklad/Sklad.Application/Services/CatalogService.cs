using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Logging;
using Sklad.Contracts.Responses;
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
using System.Net;

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

        public async Task<OperationResult<Resource>> CreateResourceAsync(Resource resource)
        {
            var result = await CreateCatalogEntityAsync(resource, _dbContext.Resources, DisplayedClassNames.Resource);
            result.Message = result.StatusCode switch
            {
                HttpStatusCode.Created => OperationResultMessages.ResourceCreated,
                HttpStatusCode.Conflict => OperationResultMessages.ResourceAlreadyExists,
                HttpStatusCode.InternalServerError => OperationResultMessages.ResourceCreationFailed,
                _ => string.Empty
            };
            return result;
        }

        public async Task<OperationResult<Resource>> UpdateResourceAsync(Resource resource)
        {
            try
            {
                _dbContext.Resources.Update(resource);
                await _dbContext.SaveChangesAsync();
                return new OperationResult<Resource>
                {
                    StatusCode = HttpStatusCode.OK,
                    Data = resource,
                    Message = OperationResultMessages.ResourceUpdated
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при обновлении ресурса с ID {resource.Id}");
                return new OperationResult<Resource>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = OperationResultMessages.ResourceUpdateFailed
                };
            }
        }

        public async Task<OperationResult> DeleteResourceAsync(int resourceId)
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
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.Conflict,
                        Message = OperationResultMessages.ResourceInUse
                    };
                }
                if (resource != null)
                {
                    _dbContext.Resources.Remove(resource);
                    await _dbContext.SaveChangesAsync();
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.OK,
                        Message = OperationResultMessages.ResourceDeleted
                    };
                }
                else
                {
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = OperationResultMessages.ResourceNotFound
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, OperationResultMessages.ResourceDeletionFailed);
                return new OperationResult
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = OperationResultMessages.ResourceDeletionFailed
                };
            }
        }

        public async Task<OperationResult> ArchiveResourceAsync(Resource resource)
        {
            var result = await ArchiveCatalogEntityAsync(resource, _dbContext.Resources);
            result.Message = result.StatusCode switch
            {
                HttpStatusCode.OK => OperationResultMessages.ResourceArchived,
                HttpStatusCode.NotFound => OperationResultMessages.ResourceNotFound,
                HttpStatusCode.InternalServerError => OperationResultMessages.ResourceArchiveFailed,
                _ => string.Empty
            };
            return result;
        }

        public async Task<List<UnitOfMeasurement>> GetUnitsOfMeasurementAsync() =>
            await _dbContext.UnitOfMeasurements.ToListAsync();

        public async Task<OperationResult<UnitOfMeasurement>> CreateUnitOfMeasurementAsync(UnitOfMeasurement unitOfMeasurement)
        {
            var result = await CreateCatalogEntityAsync(unitOfMeasurement, _dbContext.UnitOfMeasurements, DisplayedClassNames.UnitOfMeasurement);
            result.Message = result.StatusCode switch
            {
                HttpStatusCode.Created => OperationResultMessages.UnitOfMeasurementCreated,
                HttpStatusCode.Conflict => OperationResultMessages.UnitOfMeasurementAlreadyExists,
                HttpStatusCode.InternalServerError => OperationResultMessages.UnitOfMeasurementCreationFailed,
                _ => string.Empty
            };
            return result;
        }

        public async Task<OperationResult<UnitOfMeasurement>> UpdateUnitOfMeasurementAsync(UnitOfMeasurement unitOfMeasurement)
        {
            try
            {
                _dbContext.UnitOfMeasurements.Update(unitOfMeasurement);
                await _dbContext.SaveChangesAsync();
                return new OperationResult<UnitOfMeasurement>
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = OperationResultMessages.UnitOfMeasurementUpdated,
                    Data = unitOfMeasurement
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, OperationResultMessages.UnitOfMeasurementUpdateFailed);
                return new OperationResult<UnitOfMeasurement>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = OperationResultMessages.UnitOfMeasurementUpdateFailed,
                    Data = unitOfMeasurement
                };
            }
        }

        public async Task<OperationResult> DeleteUnitOfMeasurementAsync(int unitOfMeasurementId)
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
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.Conflict,
                        Message = OperationResultMessages.UnitOfMeasurementInUse,
                    };
                }
                if (unit != null)
                {
                    _dbContext.UnitOfMeasurements.Remove(unit);
                    await _dbContext.SaveChangesAsync();
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.OK,
                        Message = OperationResultMessages.UnitOfMeasurementDeleted,
                    };
                }
                else
                {
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = OperationResultMessages.UnitOfMeasurementNotFound,
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при удалении единицы измерения с ID {unitOfMeasurementId}");
                return new OperationResult
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = OperationResultMessages.UnitOfMeasurementDeletionFailed,
                };
            }
        }

        public async Task<OperationResult> ArchiveUnitOfMeasurementAsync(UnitOfMeasurement unit)
        {
            var result = await ArchiveCatalogEntityAsync(unit, _dbContext.UnitOfMeasurements);
            result.Message = result.StatusCode switch
            {
                HttpStatusCode.OK => OperationResultMessages.UnitOfMeasurementArchived,
                HttpStatusCode.NotFound => OperationResultMessages.UnitOfMeasurementNotFound,
                HttpStatusCode.InternalServerError => OperationResultMessages.UnitOfMeasurementArchiveFailed,
                _ => string.Empty
            };
            return result;
        }

        public async Task<List<Client>> GetClientsAsync() =>
            await _dbContext.Clients.ToListAsync();

        public async Task<OperationResult<Client>> CreateClientAsync(Client client)
        {
            var result = await CreateCatalogEntityAsync(client, _dbContext.Clients, DisplayedClassNames.Client);
            result.Message = result.StatusCode switch
            {
                HttpStatusCode.Created => OperationResultMessages.ClientCreated,
                HttpStatusCode.Conflict => OperationResultMessages.ClientAlreadyExists,
                HttpStatusCode.InternalServerError => OperationResultMessages.ClientCreationFailed,
                _ => string.Empty
            };
            return result;
        }

        public async Task<OperationResult<Client>> UpdateClientAsync(Client client)
        {
            try
            {
                _dbContext.Clients.Update(client);
                await _dbContext.SaveChangesAsync();
                return new OperationResult<Client>
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = OperationResultMessages.ClientCreated,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, OperationResultMessages.ClientCreationFailed);
                return new OperationResult<Client>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = OperationResultMessages.ClientCreationFailed,
                    Data = client,
                };
            }
        }

        public async Task<OperationResult> DeleteClientAsync(int clientId)
        {
            try
            {
                var client = await _dbContext.Clients.FindAsync(clientId);
                var goodsIssueDocuments = await _dbContext.GoodsIssueDocuments
                    .Where(d => d.ClientId == clientId)
                    .ToListAsync();
                if (goodsIssueDocuments.Any())
                {
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.Conflict,
                        Message = OperationResultMessages.ClientInUse,
                    };
                }
                if (client != null)
                {
                    _dbContext.Clients.Remove(client);
                    await _dbContext.SaveChangesAsync();
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.OK,
                        Message = OperationResultMessages.ClientDeleted,
                    };
                }
                else
                {
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = OperationResultMessages.ClientNotFound,
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, OperationResultMessages.ClientDeletionFailed);
                return new OperationResult
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = OperationResultMessages.ClientDeletionFailed,
                };
            }
        }

        public async Task<OperationResult> ArchiveClientAsync(Client client)
        {
            var result = await ArchiveCatalogEntityAsync(client, _dbContext.Clients);
            result.Message = result.StatusCode switch
            {
                HttpStatusCode.OK => OperationResultMessages.ClientArchived,
                HttpStatusCode.NotFound => OperationResultMessages.ClientNotFound,
                HttpStatusCode.InternalServerError => OperationResultMessages.ClientArchiveFailed,
                _ => string.Empty
            };
            return result;
        }

        private async Task<OperationResult<TEntity>> CreateCatalogEntityAsync<TEntity>(
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
                    return new OperationResult<TEntity>
                    {
                        StatusCode = HttpStatusCode.Created,
                        Data = entity
                    };
                }
                else
                {
                    return new OperationResult<TEntity>
                    {
                        StatusCode = HttpStatusCode.Conflict,
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при создании {entityDisplayName.ToLower()}");
                return new OperationResult<TEntity>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                };
            }
        }

        private async Task<OperationResult> ArchiveCatalogEntityAsync<TEntity>(
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
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.OK,
                    };
                }
                return new OperationResult
                {
                    StatusCode = HttpStatusCode.NotFound,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при архивировании сущности {typeof(TEntity).Name}");
                return new OperationResult
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                };
            }
        }
    }
}
