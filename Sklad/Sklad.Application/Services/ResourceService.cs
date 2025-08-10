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

        public async Task<OperationResult<Resource>> CreateResourceAsync(Resource resource)
        {
            if (string.IsNullOrWhiteSpace(resource.Name))
            {
                return new OperationResult<Resource>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = _messages[MessageKeyEnum.NameRequired]
                };
            }
            var result = await _catalogService.CreateCatalogEntityAsync(resource, _dbContext.Resources, DisplayedClassNames.Resource);
            result.Message = result.StatusCode switch
            {
                HttpStatusCode.Created => _messages[MessageKeyEnum.Created],
                HttpStatusCode.Conflict => _messages[MessageKeyEnum.AlreadyExists],
                HttpStatusCode.InternalServerError => _messages[MessageKeyEnum.CreationFailed],
                _ => string.Empty
            };
            return result;
        }

        public async Task<OperationResult<Resource>> UpdateResourceAsync(Resource resource)
        {
            if (string.IsNullOrWhiteSpace(resource.Name))
            {
                return new OperationResult<Resource>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = _messages[MessageKeyEnum.NameRequired]
                };
            }
            try
            {
                _dbContext.Resources.Update(resource);
                await _dbContext.SaveChangesAsync();
                return new OperationResult<Resource>
                {
                    StatusCode = HttpStatusCode.OK,
                    Data = resource,
                    Message = _messages[MessageKeyEnum.Updated]
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при обновлении ресурса с ID {resource.Id}");
                return new OperationResult<Resource>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = _messages[MessageKeyEnum.UpdateFailed]
                };
            }
        }

        public async Task<OperationResult> DeleteResourceAsync(int resourceId)
        {
            try
            {
                var resource = await _dbContext.Resources.FindAsync(resourceId);
                var ReceiptResources = await _dbContext.ReceiptResources
                    .Where(r => r.ResourceId == resourceId)
                    .ToListAsync();
                var ShipmentResources = await _dbContext.ShipmentResources
                    .Where(r => r.ResourceId == resourceId)
                    .ToListAsync();
                if (ReceiptResources.Any() || ShipmentResources.Any())
                {
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.Conflict,
                        Message = _messages[MessageKeyEnum.InUse]
                    };
                }
                if (resource != null)
                {
                    _dbContext.Resources.Remove(resource);
                    await _dbContext.SaveChangesAsync();
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.OK,
                        Message = _messages[MessageKeyEnum.Deleted]
                    };
                }
                else
                {
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = _messages[MessageKeyEnum.NotFound]
                    };
                }
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

        public async Task<OperationResult> DeleteMultipleResourceAsync(int[] resourceIds)
        {
            var tasks = resourceIds.Select(async id => await DeleteResourceAsync(id));
            var results = await Task.WhenAll(tasks);
            var successCount = results.Count(r => r.StatusCode == HttpStatusCode.OK);
            var conflictCount = results.Count(r => r.StatusCode == HttpStatusCode.Conflict);
            var notFoundCount = results.Count(r => r.StatusCode == HttpStatusCode.NotFound);
            var failureCount = results.Count(r => r.StatusCode == HttpStatusCode.InternalServerError);
            var sb = new StringBuilder();
            var statusCode = HttpStatusCode.InternalServerError;
            if (failureCount > 0)
            {
                sb.AppendLine($"{_messages[MessageKeyEnum.DeletionFailed]}: {failureCount}");
            }
            if (notFoundCount > 0)
            {
                statusCode = HttpStatusCode.NotFound;
                sb.AppendLine($"{_messages[MessageKeyEnum.NotFound]}: {notFoundCount}");
            }
            if (conflictCount > 0)
            {
                statusCode = HttpStatusCode.Conflict;
                sb.AppendLine($"{_messages[MessageKeyEnum.InUse]}: {conflictCount}");
            }
            if (successCount > 0)
            {
                statusCode = HttpStatusCode.OK;
                sb.AppendLine($"{_messages[MessageKeyEnum.Deleted]}: {successCount}");
            }
            return new OperationResult
            {
                StatusCode = statusCode,
                Message = sb.ToString(),
            };
        }

        public async Task<OperationResult> ArchiveResourceAsync(Resource resource)
        {
            var result = await _catalogService.ArchiveCatalogEntityAsync(resource, _dbContext.Resources);
            result.Message = result.StatusCode switch
            {
                HttpStatusCode.OK => _messages[MessageKeyEnum.Archived],
                HttpStatusCode.NotFound => _messages[MessageKeyEnum.NotFound],
                HttpStatusCode.InternalServerError => _messages[MessageKeyEnum.ArchiveFailed],
                _ => string.Empty
            };
            return result;
        }

        public async Task<OperationResult> ArchiveMultipleResourceAsync(Resource[] resources) =>
            await _catalogService.ArchiveMultipleEntitiesAsync(resources, _dbContext.Resources, ArchiveResourceAsync);
    }
}
