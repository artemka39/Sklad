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
    }
}
