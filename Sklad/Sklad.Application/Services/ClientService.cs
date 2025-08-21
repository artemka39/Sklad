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
            await _catalogService.GetCatalogEntitiesAsync(state, _dbContext.Clients);

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
            var result = await _catalogService.CreateCatalogEntityAsync(client, _dbContext.Clients);
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
            return await _catalogService.UpdateCatalogEntityAsync(client, _dbContext.Clients);
        }

        public async Task<OperationResult> DeleteClientAsync(int clientId)
        {
            try
            {
                var client = await _dbContext.Clients.FindAsync(clientId);
                if (client == null)
                {
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Message = _messages[MessageKeyEnum.NotFound]
                    };
                }
                var shipmentDocuments = await _dbContext.ShipmentDocuments
                    .Where(d => d.ClientId == clientId)
                    .ToListAsync();
                if (shipmentDocuments.Any())
                {
                    return new OperationResult
                    {
                        StatusCode = HttpStatusCode.Conflict,
                        Message = _messages[MessageKeyEnum.InUse],
                    };
                }
                return await _catalogService.DeleteCatalogEntityAsync(client, _dbContext.Clients);
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

        public async Task<OperationResult> ArchiveClientAsync(Client client) => 
            await _catalogService.ArchiveCatalogEntityAsync(client, _dbContext.Clients);
    }
}
