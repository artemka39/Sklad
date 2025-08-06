using Microsoft.AspNetCore.Mvc;
using Sklad.Domain.Interfaces;
using Sklad.Domain.Models;
using Sklad.Contracts.Requests;

namespace Sklad.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StorageController : ControllerBase
    {
        private readonly IStorageService _storageService;
        public StorageController(IStorageService storageService)
        {
            _storageService = storageService;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetStorageBalance([FromQuery] int? resourceId, [FromQuery] int? unitId)
        {
            var storageBalance = await _storageService.GetStorageBalanceAsync(resourceId, unitId);
            return Ok(storageBalance);
        }

        [HttpGet("receipts")]
        public async Task<IActionResult> GetGoodsReceiptDocuments()
        {
            var goodsReceiptDocuments = await _storageService.GetGoodsReceiptDocumentsAsync();
            return Ok(goodsReceiptDocuments);
        }

        [HttpPost("receipts")]
        public async Task<IActionResult> CreateGoodsReceiptDocument([FromBody] CreateGoodsReceiptDocumentRequest request)
        {
            await _storageService.CreateGoodsReceiptDocumentAsync(request);
            return Ok();
        }

        [HttpDelete("receipts/{documentId}")]
        public async Task<IActionResult> DeleteGoodsReceiptDocument(int documentId)
        {
            await _storageService.DeleteGoodsReceiptDocument(documentId);
            return NoContent();
        }

        [HttpGet("shipments")]
        public async Task<IActionResult> GetGoodsIssueDocuments()
        {
            var goodsIssueDocuments = await _storageService.GetGoodsIssueDocumentsAsync();
            return Ok(goodsIssueDocuments);
        }

        [HttpPost("shipments")]
        public async Task<IActionResult> CreateGoodsIssueDocument([FromBody] CreateGoodsIssueDocumentRequest request)
        {
            await _storageService.CreateGoodsIssueDocumentAsync(request);
            return Ok();
        }
    }
}