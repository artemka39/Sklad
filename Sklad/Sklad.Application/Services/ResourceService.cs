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
    public class ResourceService : IResourceService
    {
        private readonly SkladDbContext _dbContext;
        private readonly ILogger<ResourceService> _logger;
        private readonly ICatalogService _catalogService;
        private readonly IReadOnlyDictionary<MessageKeyEnum, string> _messages;
        public ResourceService(SkladDbContext dbContext, ILogger<ResourceService> logger, ICatalogService catalogService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _catalogService = catalogService;
            _messages = OperationResultMessages.GetMessagesFor(typeof(Resource));
        }

        public async Task<List<Resource>> GetResourcesAsync(CatalogEntityStateEnum? state) =>
            await _dbContext.Resources.Where(r => !state.HasValue || r.State == state.Value).ToListAsync();

        public async Task<OperationResult<Resource>> CreateResourceAsync(Resource resource) => 
            await _catalogService.CreateCatalogEntityAsync(resource, _dbContext.Resources);

        public async Task<OperationResult<Resource>> UpdateResourceAsync(Resource resource) =>
            await _catalogService.UpdateCatalogEntityAsync(resource, _dbContext.Resources);

        public async Task<OperationResult> DeleteResourceAsync(int resourceId)
        {
            try
            {
                var resource = await _dbContext.Resources.FindAsync(resourceId);
                if (resource == null)
                {
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = _messages[MessageKeyEnum.NotFound]
                    };
                }
                var receiptResources = await _dbContext.ReceiptResources
                    .Where(r => r.ResourceId == resourceId)
                    .ToListAsync();
                var shipmentResources = await _dbContext.ShipmentResources
                    .Where(r => r.ResourceId == resourceId)
                    .ToListAsync();
                if (receiptResources.Any() || shipmentResources.Any())
                {
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.Conflict,
                        Message = _messages[MessageKeyEnum.InUse]
                    };
                }
                return await _catalogService.DeleteCatalogEntityAsync(resource, _dbContext.Resources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _messages[MessageKeyEnum.DeletionFailed]);
                return new OperationResult
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = _messages[MessageKeyEnum.DeletionFailed]
                };
            }
        }

        public async Task<OperationResult> DeleteMultipleResourcesAsync(int[] resourceIds)
        {
            var resources = await _dbContext.Resources
                .Where(r => resourceIds.Contains(r.Id))
                .ToListAsync();
            var notFoundCount = resourceIds.Length - resources.Count;
            var inUseResources = resources
                .Where(r =>
                    _dbContext.ReceiptResources.Any(rr => rr.ResourceId == r.Id) ||
                    _dbContext.ShipmentResources.Any(rs => rs.ResourceId == r.Id))
                .ToList();
            return await _catalogService.DeleteMultipleEntitiesAsync(_dbContext.Resources, inUseResources, resources, notFoundCount);
        }

        public async Task<OperationResult> ArchiveResourceAsync(Resource resource) => 
            await _catalogService.ArchiveCatalogEntityAsync(resource, _dbContext.Resources);

        public async Task<OperationResult> ArchiveMultipleResourcesAsync(Resource[] resources) =>
            await _catalogService.ArchiveMultipleEntitiesAsync(resources, _dbContext.Resources);
    }
}
