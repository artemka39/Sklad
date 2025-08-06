using Sklad.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sklad.Contracts.Requests;
using Sklad.Contracts.Responses;

namespace Sklad.Domain.Interfaces
{
    public interface IStorageService
    {
        Task<List<Balance>> GetStorageBalanceAsync(int? resourceId, int? unitId);

        Task<List<GoodsReceiptDocument>> GetGoodsReceiptDocumentsAsync();
        Task<OperationResult<GoodsReceiptDocument>> CreateGoodsReceiptDocumentAsync(CreateGoodsReceiptDocumentRequest request);
        Task<OperationResult<GoodsReceiptDocument>> UpdateGoodsReceiptDocumentAsync(UpdateGoodsReceiptDocumentRequest request);
        Task<OperationResult> DeleteGoodsReceiptDocument(int documentId);

        Task<List<GoodsIssueDocument>> GetGoodsIssueDocumentsAsync();
        Task<OperationResult<GoodsIssueDocument>> CreateGoodsIssueDocumentAsync(CreateGoodsIssueDocumentRequest request);
        Task<OperationResult<GoodsIssueDocument>> UpdateGoodsIssueDocumentAsync(UpdateGoodsIssueDocumentRequest request);
        Task<OperationResult> DeleteGoodsIssueDocumentAsync(int documentId);
        Task<OperationResult<GoodsIssueDocument>> SignGoodsIssueDocumentAsync(int documentId);
        Task<OperationResult<GoodsIssueDocument>> WithdrawGoodsIssueDocumentAsync(int documentId);
    }
}
