using Sklad.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sklad.Domain.Interfaces
{
    public interface IStorageService
    {
        Task<List<GoodsReceiptDocument>> GetGoodsReceiptDocumentsAsync();
        Task CreateGoodsReceiptDocumentAsync(GoodsReceiptDocument document, List<InboundResource> resources);
        Task DeleteGoodsReceiptDocument(Guid documentId);

        Task<List<GoodsIssueDocument>> GetGoodsIssueDocumentsAsync();
        Task CreateGoodsIssueDocumentAsync(GoodsIssueDocument document, List<OutboundResource> resources);

        Task<List<Resource>> GetResourcesAsync();
        Task CreateResourceAsync(Resource resource);

        Task<List<UnitOfMeasurement>> GetUnitOfMeasurementsAsync();
        Task CreateUnitOfMeasurementAsync(UnitOfMeasurement unitOfMeasurement);

        Task<List<Client>> GetClientsAsync();
        Task CreateClientAsync(Client client);
    }
}
