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

        public async Task<List<TEntity>> GetCatalogEntitiesAsync<TEntity>(
            CatalogEntityStateEnum? state,
            DbSet<TEntity> dbSet) where TEntity : class, ICatalogEntity
        {
            var messages = GetMessages<TEntity>();
            try
            {
                var query = dbSet.AsQueryable();
                if (state.HasValue)
                {
                    query = query.Where(e => e.State == state.Value);
                }
                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, messages[MessageKeyEnum.LoadingFailed]);
                return new List<TEntity>();
            }
        }

        public async Task<OperationResult<TEntity>> CreateCatalogEntityAsync<TEntity>(
            TEntity entity,
            DbSet<TEntity> dbSet)
            where TEntity : class, ICatalogEntity
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
                var existingEntity = await dbSet.FirstOrDefaultAsync(e => e.Id != entity.Id && e.Name == entity.Name);
                if (existingEntity == null)
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
                    if (existingEntity.State == CatalogEntityStateEnum.Archived)
                    {
                        return new OperationResult
                        {
                            StatusCode = HttpStatusCode.BadRequest,
                            Message = messages[MessageKeyEnum.AlreadyArchived]
                        };
                    }
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

        private IReadOnlyDictionary<MessageKeyEnum, string> GetMessages<TEntity>()
            => OperationResultMessages.GetMessagesFor(typeof(TEntity));
    }
}
