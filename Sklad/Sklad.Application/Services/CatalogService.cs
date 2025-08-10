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
            DbSet<TEntity> dbSet,
            string entityDisplayName)
            where TEntity : class, ICatalogEntity
        {
            try
            {
                var messages = OperationResultMessages.GetMessagesFor(typeof(TEntity));
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

        //public async Task<OperationResult> DeleteMultipleEntitiesAsync(
        //    )
        //{

        //}

        public async Task<OperationResult> ArchiveCatalogEntityAsync<TEntity>(
            TEntity entity,
            DbSet<TEntity> dbSet)
            where TEntity : class, ICatalogEntity
        {
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

        public async Task<OperationResult> ArchiveMultipleEntitiesAsync<TEntity>(
            TEntity[] entities,
            DbSet<TEntity> dbSet,
            Func<TEntity, Task<OperationResult>> func) where TEntity : class, ICatalogEntity
        {
            var messages = OperationResultMessages.GetMessagesFor(typeof(TEntity));
            var tasks = entities.Select(async r => await func(r));
            var results = await Task.WhenAll(tasks);
            var successCount = results.Count(r => r.StatusCode == HttpStatusCode.OK);
            var notFoundCount = results.Count(r => r.StatusCode == HttpStatusCode.NotFound);
            var failureCount = results.Count(r => r.StatusCode == HttpStatusCode.InternalServerError);
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
            if (successCount > 0)
            {
                statusCode = HttpStatusCode.OK;
                sb.AppendLine($"{messages[MessageKeyEnum.Archived]}: {successCount}");
            }
            return new OperationResult
            {
                StatusCode = statusCode,
                Message = sb.ToString(),
            };
        }
    }
}
