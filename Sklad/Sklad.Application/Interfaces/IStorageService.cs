using Sklad.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sklad.Contracts.Requests;
using Sklad.Contracts.Responses;
using Sklad.Contracts.Dtos;

namespace Sklad.Application.Interfaces
{
    public interface IStorageService
    {
        Task<List<Balance>> GetStorageBalanceAsync(int? resourceId, int? unitId);

        Task<List<ReceiptDocument>> GetReceiptDocumentsAsync(DocumentFilterDto filters);
        Task<OperationResult<ReceiptDocument>> CreateReceiptDocumentAsync(CreateReceiptDocumentRequest request);
        Task<OperationResult<ReceiptDocument>> UpdateReceiptDocumentAsync(UpdateReceiptDocumentRequest request);
        Task<OperationResult> DeleteReceiptDocument(int documentId);

        Task<List<ShipmentDocument>> GetShipmentDocumentsAsync(DocumentFilterDto filters);
        Task<OperationResult<ShipmentDocument>> CreateShipmentDocumentAsync(CreateShipmentDocumentRequest request);
        Task<OperationResult<ShipmentDocument>> UpdateShipmentDocumentAsync(UpdateShipmentDocumentRequest request);
        Task<OperationResult> DeleteShipmentDocumentAsync(int documentId);
        Task<OperationResult<ShipmentDocument>> SignShipmentDocumentAsync(int documentId);
        Task<OperationResult<ShipmentDocument>> WithdrawShipmentDocumentAsync(int documentId);
    }
}
