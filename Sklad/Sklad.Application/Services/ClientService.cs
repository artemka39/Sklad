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
    public class ClientService : IClientService
    {
        private readonly SkladDbContext _dbContext;
        private readonly ILogger<ClientService> _logger;
        private readonly ICatalogService _catalogService;
        private readonly IReadOnlyDictionary<MessageKeyEnum, string> _messages;
        public ClientService(SkladDbContext dbContext, ILogger<ClientService> logger, ICatalogService catalogService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _catalogService = catalogService;
            _messages = OperationResultMessages.GetMessagesFor(typeof(Client));
        }

        public async Task<List<Client>> GetClientsAsync(CatalogEntityStateEnum? state) =>
            await _dbContext.Clients.Where(r => !state.HasValue || r.State == state.Value).ToListAsync();

        public async Task<OperationResult<Client>> CreateClientAsync(Client client)
        {
            if (string.IsNullOrWhiteSpace(client.Name))
            {
                return new OperationResult<Client>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = _messages[MessageKeyEnum.NameRequired]
                };
            }
            if (string.IsNullOrWhiteSpace(client.Address))
            {
                return new OperationResult<Client>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = _messages[MessageKeyEnum.AddressRequired]
                };
            }
            var result = await _catalogService.CreateCatalogEntityAsync(client, _dbContext.Clients, DisplayedClassNames.Client);
            result.Message = result.StatusCode switch
            {
                HttpStatusCode.Created => _messages[MessageKeyEnum.Created],
                HttpStatusCode.Conflict => _messages[MessageKeyEnum.AlreadyExists],
                HttpStatusCode.InternalServerError => _messages[MessageKeyEnum.CreationFailed],
                _ => string.Empty
            };
            return result;
        }

        public async Task<OperationResult<Client>> UpdateClientAsync(Client client)
        {
            if (string.IsNullOrWhiteSpace(client.Name))
            {
                return new OperationResult<Client>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = _messages[MessageKeyEnum.NameRequired]
                };
            }
            if (string.IsNullOrWhiteSpace(client.Address))
            {
                return new OperationResult<Client>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = _messages[MessageKeyEnum.AddressRequired]
                };
            }
            try
            {
                _dbContext.Clients.Update(client);
                await _dbContext.SaveChangesAsync();
                return new OperationResult<Client>
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = _messages[MessageKeyEnum.Created],
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _messages[MessageKeyEnum.CreationFailed]);
                return new OperationResult<Client>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = _messages[MessageKeyEnum.CreationFailed],
                    Data = client,
                };
            }
        }

        public async Task<OperationResult> DeleteClientAsync(int clientId)
        {
            try
            {
                var client = await _dbContext.Clients.FindAsync(clientId);
                var ShipmentDocuments = await _dbContext.ShipmentDocuments
                    .Where(d => d.ClientId == clientId)
                    .ToListAsync();
                if (ShipmentDocuments.Any())
                {
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.Conflict,
                        Message = _messages[MessageKeyEnum.InUse],
                    };
                }
                if (client != null)
                {
                    _dbContext.Clients.Remove(client);
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
                _logger.LogError(ex, _messages[MessageKeyEnum.DeletionFailed]);
                return new OperationResult
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = _messages[MessageKeyEnum.DeletionFailed],
                };
            }
        }

        public async Task<OperationResult> DeleteMultipleClientAsync(int[] clientIds)
        {
            var tasks = clientIds.Select(async id => await DeleteClientAsync(id));
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

        public async Task<OperationResult> ArchiveClientAsync(Client client)
        {
            var result = await _catalogService.ArchiveCatalogEntityAsync(client, _dbContext.Clients);
            result.Message = result.StatusCode switch
            {
                HttpStatusCode.OK => _messages[MessageKeyEnum.Archived],
                HttpStatusCode.NotFound => _messages[MessageKeyEnum.NotFound],
                HttpStatusCode.InternalServerError => _messages[MessageKeyEnum.ArchiveFailed],
                _ => string.Empty
            };
            return result;
        }

        public async Task<OperationResult> ArchiveMultipleClientAsync(Client[] clients) =>
            await _catalogService.ArchiveMultipleEntitiesAsync(clients, _dbContext.Clients, ArchiveClientAsync);
    }
}
