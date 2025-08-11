using Sklad.Contracts.Responses;
using Sklad.Domain.Enums;
using Sklad.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sklad.Application.Interfaces
{
    public interface IClientService
    {
        Task<List<Client>> GetClientsAsync(CatalogEntityStateEnum? state);
        Task<OperationResult<Client>> CreateClientAsync(Client client);
        Task<OperationResult<Client>> UpdateClientAsync(Client client);
        Task<OperationResult> DeleteClientAsync(int clientId);
        Task<OperationResult> DeleteMultipleClientsAsync(int[] clientIds);
        Task<OperationResult> ArchiveClientAsync(Client client);
        Task<OperationResult> ArchiveMultipleClientsAsync(Client[] clients);
    }
}
