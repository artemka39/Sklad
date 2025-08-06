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
            var response = await _storageService.CreateGoodsReceiptDocumentAsync(request);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("receipts/{documentId}")]
        public async Task<IActionResult> DeleteGoodsReceiptDocument(int documentId)
        {
            var response = await _storageService.DeleteGoodsReceiptDocument(documentId);
            return StatusCode((int)response.StatusCode, response);
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
            var response = await _storageService.CreateGoodsIssueDocumentAsync(request);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPatch("shipments/sing/{documentId}")]
        public async Task<IActionResult> SignGoodsIssueDocument(int documentId)
        {
            var response = await _storageService.SignGoodsIssueDocumentAsync(documentId);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPatch("shipments/withdraw/{documentId}")]
        public async Task<IActionResult> WithdrawGoodsIssueDocument(int documentId)
        {
            var response = await _storageService.WithdrawGoodsIssueDocumentAsync(documentId);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}