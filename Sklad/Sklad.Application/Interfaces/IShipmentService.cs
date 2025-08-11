using Sklad.Contracts.Dtos;
using Sklad.Contracts.Requests;
using Sklad.Contracts.Responses;
using Sklad.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sklad.Application.Interfaces
{
    public interface IShipmentService
    {
        Task<List<ShipmentDocument>> GetShipmentDocumentsAsync(DocumentFilterDto filters);
        Task<OperationResult<ShipmentDocument>> CreateShipmentDocumentAsync(CreateShipmentDocumentRequest request);
        Task<OperationResult<ShipmentDocument>> UpdateShipmentDocumentAsync(UpdateShipmentDocumentRequest request);
        Task<OperationResult> DeleteShipmentDocumentAsync(int documentId);
        Task<OperationResult<ShipmentDocument>> SignShipmentDocumentAsync(int documentId);
        Task<OperationResult<ShipmentDocument>> WithdrawShipmentDocumentAsync(int documentId);
    }
}
