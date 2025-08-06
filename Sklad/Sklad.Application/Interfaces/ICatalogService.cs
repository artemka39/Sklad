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
        Task CreateResourceAsync(Resource resource);
        Task UpdateResourceAsync(Resource resource);
        Task DeleteResourceAsync(int resourceId);

        Task<List<UnitOfMeasurement>> GetUnitOfMeasurementsAsync();
        Task CreateUnitOfMeasurementAsync(UnitOfMeasurement unitOfMeasurement);
        Task UpdateUnitOfMeasurementAsync(UnitOfMeasurement unitOfMeasurement);
        Task DeleteUnitOfMeasurementAsync(int unitOfMeasurementId);

        Task<List<Client>> GetClientsAsync();
        Task CreateClientAsync(Client client);
        Task UpdateClientAsync(Client client);
        Task DeleteClientAsync(int clientId);
    }
}
