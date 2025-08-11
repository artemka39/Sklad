using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Logging;
using Sklad.Contracts.Responses;
using Sklad.Domain.Constants;
using Sklad.Domain.Enums;
using Sklad.Domain.Models;
using Sklad.Persistence;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Sklad.Application.Interfaces;

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

        public async Task<OperationResult<TEntity>> CreateCatalogEntityAsync<TEntity>(
            TEntity entity,
            DbSet<TEntity> dbSet)
            where TEntity : class, ICatalogEntity
        {
            var messages = GetMessages<TEntity>();
            try
            {
                if (string.IsNullOrWhiteSpace(entity.Name))
                {
                    return new OperationResult<TEntity>
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = messages[MessageKeyEnum.NameRequired]
                    };
                }
                var existingEntity = await dbSet.FirstOrDefaultAsync(e => e.Name == entity.Name);
                if (existingEntity == null)
                {
                    entity.State = CatalogEntityStateEnum.Active;
                    await dbSet.AddAsync(entity);
                    await _dbContext.SaveChangesAsync();
                    return new OperationResult<TEntity>
                    {
                        StatusCode = HttpStatusCode.Created,
                        Message = messages[MessageKeyEnum.Created],
                        Data = entity
                    };
                }
                else
                {
                    return new OperationResult<TEntity>
                    {
                        StatusCode = HttpStatusCode.Conflict,
                        Message = messages[MessageKeyEnum.AlreadyExists]
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, messages[MessageKeyEnum.CreationFailed]);
                return new OperationResult<TEntity>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = messages[MessageKeyEnum.CreationFailed]
                };
            }
        }

        public async Task<OperationResult<TEntity>> UpdateCatalogEntityAsync<TEntity>(
            TEntity entity,
            DbSet<TEntity> dbSet) where TEntity : class, ICatalogEntity
        {
            var messages = GetMessages<TEntity>();
            if (string.IsNullOrWhiteSpace(entity.Name))
            {
                return new OperationResult<TEntity>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = messages[MessageKeyEnum.NameRequired]
                };
            }
            try
            {
                dbSet.Update(entity);
                await _dbContext.SaveChangesAsync();
                return new OperationResult<TEntity>
                {
                    StatusCode = HttpStatusCode.OK,
                    Data = entity,
                    Message = messages[MessageKeyEnum.Updated]
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, messages[MessageKeyEnum.UpdateFailed]);
                return new OperationResult<TEntity>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = messages[MessageKeyEnum.UpdateFailed]
                };
            }
        }

        public async Task<OperationResult> DeleteCatalogEntityAsync<TEntity>(
            TEntity entity, 
            DbSet<TEntity> dbSet) where TEntity : class, ICatalogEntity
        {
            var messages = GetMessages<TEntity>();
            try
            {
                if (entity != null)
                {
                    dbSet.Remove(entity);
                    await _dbContext.SaveChangesAsync();
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.OK,
                        Message = messages[MessageKeyEnum.Deleted],
                    };
                }
                else
                {
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = messages[MessageKeyEnum.NotFound],
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, messages[MessageKeyEnum.DeletionFailed]);
                return new OperationResult
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = messages[MessageKeyEnum.DeletionFailed],
                };
            }
        }

        public async Task<OperationResult> DeleteMultipleEntitiesAsync<TEntity>(
            DbSet<TEntity> dbSet,
            List<TEntity> inUseEntities,
            List<TEntity> entities,
            int notFoundCount) where TEntity : class, ICatalogEntity
        {
            var messages = GetMessages<TEntity>();
            var conflictCount = inUseEntities.Count;
            var deletableEntities = entities.Except(inUseEntities).ToList();
            var successCount = deletableEntities.Count;
            if (successCount > 0)
            {
                dbSet.RemoveRange(deletableEntities);
                await _dbContext.SaveChangesAsync();
            }
            var sb = new StringBuilder();
            var statusCode = HttpStatusCode.InternalServerError;
            if (notFoundCount > 0)
            {
                statusCode = HttpStatusCode.NotFound;
                sb.AppendLine($"{messages[MessageKeyEnum.NotFound]}: {notFoundCount}");
            }
            if (conflictCount > 0)
            {
                statusCode = HttpStatusCode.Conflict;
                sb.AppendLine($"{messages[MessageKeyEnum.InUse]}: {conflictCount}");
            }
            if (successCount > 0)
            {
                statusCode = HttpStatusCode.OK;
                sb.AppendLine($"{messages[MessageKeyEnum.Deleted]}: {successCount}");
            }
            return new OperationResult
            {
                StatusCode = statusCode,
                Message = sb.ToString()
            };
        }

        public async Task<OperationResult> ArchiveCatalogEntityAsync<TEntity>(
            TEntity entity,
            DbSet<TEntity> dbSet)
            where TEntity : class, ICatalogEntity
        {
            var messages = GetMessages<TEntity>();
            try
            {
                var set = _dbContext.Set<TEntity>();
                var existingEntity = await dbSet.FirstOrDefaultAsync(e => e.Name == entity.Name);
                if (existingEntity != null)
                {
                    existingEntity.State = CatalogEntityStateEnum.Archived;
                    await _dbContext.SaveChangesAsync();
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.OK,
                        Message = messages[MessageKeyEnum.Archived]
                    };
                }
                return new OperationResult
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = messages[MessageKeyEnum.NotFound]
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, messages[MessageKeyEnum.ArchiveFailed]);
                return new OperationResult
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = messages[MessageKeyEnum.ArchiveFailed]
                };
            }
        }

        public async Task<OperationResult> ArchiveMultipleEntitiesAsync<TEntity>(
            TEntity[] entities,
            DbSet<TEntity> dbSet)
            where TEntity : class, ICatalogEntity
        {
            var messages = OperationResultMessages.GetMessagesFor(typeof(TEntity));
            var names = entities.Select(e => e.Name).Distinct().ToList();
            var foundEntities = await dbSet
                .Where(e => names.Contains(e.Name))
                .ToListAsync();
            var totalCount = names.Count;
            var foundCount = foundEntities.Count;
            var notFoundCount = totalCount - foundCount;
            var failureCount = 0;
            try
            {
                foreach (var entity in foundEntities)
                {
                    entity.State = CatalogEntityStateEnum.Archived;
                }
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, messages[MessageKeyEnum.ArchiveFailed]);
                failureCount = foundCount;
                foundCount = 0;
            }

            var sb = new StringBuilder();
            var statusCode = HttpStatusCode.InternalServerError;

            if (failureCount > 0)
            {
                sb.AppendLine($"{messages[MessageKeyEnum.ArchiveFailed]}: {failureCount}");
            }
            if (notFoundCount > 0)
            {
                statusCode = HttpStatusCode.NotFound;
                sb.AppendLine($"{messages[MessageKeyEnum.NotFound]}: {notFoundCount}");
            }
            if (foundCount > 0)
            {
                statusCode = HttpStatusCode.OK;
                sb.AppendLine($"{messages[MessageKeyEnum.Archived]}: {foundCount}");
            }
            return new OperationResult
            {
                StatusCode = statusCode,
                Message = sb.ToString(),
            };
        }

        private IReadOnlyDictionary<MessageKeyEnum, string> GetMessages<TEntity>()
            => OperationResultMessages.GetMessagesFor(typeof(TEntity));

    }
}
