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
    public interface IReceiptService
    {
        Task<List<ReceiptDocument>> GetReceiptDocumentsAsync(DocumentFilterDto filters);
        Task<OperationResult<ReceiptDocument>> CreateReceiptDocumentAsync(CreateReceiptDocumentRequest request);
        Task<OperationResult<ReceiptDocument>> UpdateReceiptDocumentAsync(UpdateReceiptDocumentRequest request);
        Task<OperationResult> DeleteReceiptDocument(int documentId);
    }
}
