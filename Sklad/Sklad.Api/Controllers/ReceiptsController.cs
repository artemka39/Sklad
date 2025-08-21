using Microsoft.AspNetCore.Mvc;
using Sklad.Application.Interfaces;
using Sklad.Contracts.Dtos;
using Sklad.Contracts.Requests;

namespace Sklad.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReceiptsController : ControllerBase
    {
        private readonly IReceiptService _receiptService;
        public ReceiptsController(IReceiptService receiptService)
        {
            _receiptService = receiptService;
        }

        [HttpGet]
        public async Task<IActionResult> GetReceiptDocuments([FromQuery] DocumentFilterDto filters)
        {
            var receiptDocuments = await _receiptService.GetReceiptDocumentsAsync(filters);
            return Ok(receiptDocuments);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReceiptDocument([FromBody] CreateReceiptDocumentRequest request)
        {
            var response = await _receiptService.CreateReceiptDocumentAsync(request);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateReceiptDocument([FromBody] UpdateReceiptDocumentRequest request)
        {
            var response = await _receiptService.UpdateReceiptDocumentAsync(request);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("{documentId}")]
        public async Task<IActionResult> DeleteReceiptDocument(int documentId)
        {
            var response = await _receiptService.DeleteReceiptDocument(documentId);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
