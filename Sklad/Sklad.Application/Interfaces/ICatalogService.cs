using Microsoft.EntityFrameworkCore;
using Sklad.Contracts.Responses;
using Sklad.Domain.Constants;
using Sklad.Domain.Enums;
using Sklad.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sklad.Application.Interfaces
{
    public interface ICatalogService
    {
        public Task<List<TEntity>> GetCatalogEntitiesAsync<TEntity>(
            CatalogEntityStateEnum? state,
            DbSet<TEntity> dbSet)
            where TEntity : class, ICatalogEntity;

        public Task<OperationResult<TEntity>> CreateCatalogEntityAsync<TEntity>(
            TEntity entity,
            DbSet<TEntity> dbSet)
            where TEntity : class, ICatalogEntity;

        public Task<OperationResult<TEntity>> UpdateCatalogEntityAsync<TEntity>(
            TEntity entity,
            DbSet<TEntity> dbSet)
            where TEntity : class, ICatalogEntity;

        public Task<OperationResult> DeleteCatalogEntityAsync<TEntity>(
            TEntity entity,
            DbSet<TEntity> dbSet)
            where TEntity : class, ICatalogEntity;

        public Task<OperationResult> ArchiveCatalogEntityAsync<TEntity>(
            TEntity entity,
            DbSet<TEntity> dbSet)
            where TEntity : class, ICatalogEntity;
    }
}
