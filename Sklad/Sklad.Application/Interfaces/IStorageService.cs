using Sklad.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sklad.Contracts.Requests;

namespace Sklad.Domain.Interfaces
{
    public interface IStorageService
    {
        Task<List<Balance>> GetStorageBalanceAsync(int? resourceId, int? unitId);

        Task<List<GoodsReceiptDocument>> GetGoodsReceiptDocumentsAsync();
        Task CreateGoodsReceiptDocumentAsync(CreateGoodsReceiptDocumentRequest request);
        Task DeleteGoodsReceiptDocument(int documentId);

        Task<List<GoodsIssueDocument>> GetGoodsIssueDocumentsAsync();
        Task CreateGoodsIssueDocumentAsync(CreateGoodsIssueDocumentRequest request);
        Task SignGoodsIssueDocumentAsync(int documentId);
        Task WithdrawGoodsIssueDocumentAsync(int documentId);
    }
}
