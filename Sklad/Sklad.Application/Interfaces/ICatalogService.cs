using Sklad.Contracts.Responses;
using Sklad.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sklad.Domain.Interfaces
{
    public interface ICatalogService
    {
        Task<List<Resource>> GetResourcesAsync();
        Task<OperationResult<Resource>> CreateResourceAsync(Resource resource);
        Task<OperationResult<Resource>> UpdateResourceAsync(Resource resource);
        Task<OperationResult> DeleteResourceAsync(int resourceId);
        Task<OperationResult> ArchiveResourceAsync(Resource resource);

        Task<List<UnitOfMeasurement>> GetUnitsOfMeasurementAsync();
        Task<OperationResult<UnitOfMeasurement>> CreateUnitOfMeasurementAsync(UnitOfMeasurement unitOfMeasurement);
        Task<OperationResult<UnitOfMeasurement>> UpdateUnitOfMeasurementAsync(UnitOfMeasurement unitOfMeasurement);
        Task<OperationResult> DeleteUnitOfMeasurementAsync(int unitOfMeasurementId);
        Task<OperationResult> ArchiveUnitOfMeasurementAsync(UnitOfMeasurement unitOfMeasurement);

        Task<List<Client>> GetClientsAsync();
        Task<OperationResult<Client>> CreateClientAsync(Client client);
        Task<OperationResult<Client>> UpdateClientAsync(Client client);
        Task<OperationResult> DeleteClientAsync(int clientId);
        Task<OperationResult> ArchiveClientAsync(Client client);
    }
}
