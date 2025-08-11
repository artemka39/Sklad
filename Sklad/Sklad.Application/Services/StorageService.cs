using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sklad.Application.Interfaces;
using Sklad.Contracts.Dtos;
using Sklad.Contracts.Requests;
using Sklad.Contracts.Responses;
using Sklad.Domain.Constants;
using Sklad.Domain.Enums;
using Sklad.Domain.Models;
using Sklad.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace Sklad.Application.Services
{
    public class StorageService : IStorageService
    {
        private readonly SkladDbContext _dbContext;

        public StorageService(SkladDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Balance>> GetStorageBalanceAsync(int? resourceId, int? unitId) =>
            await _dbContext.Balances
                .Where(b => (!resourceId.HasValue || b.ResourceId == resourceId) &&
                            (!unitId.HasValue || b.UnitId == unitId))
                .Include(b => b.Resource)
                .Include(b => b.Unit)
                .ToListAsync();
    }
}