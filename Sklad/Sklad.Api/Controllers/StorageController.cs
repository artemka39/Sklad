using Microsoft.AspNetCore.Mvc;
using Sklad.Domain.Models;
using Sklad.Contracts.Requests;
using Sklad.Contracts.Dtos;
using Sklad.Application.Interfaces;

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
        public async Task<IActionResult> GetReceiptDocuments([FromQuery] DocumentFilterDto filters)
        {
            var ReceiptDocuments = await _storageService.GetReceiptDocumentsAsync(filters);
            return Ok(ReceiptDocuments);
        }

        [HttpPost("receipts")]
        public async Task<IActionResult> CreateReceiptDocument([FromBody] CreateReceiptDocumentRequest request)
        {
            var response = await _storageService.CreateReceiptDocumentAsync(request);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut("receipts")]
        public async Task<IActionResult> UpdateReceiptDocument([FromBody] UpdateReceiptDocumentRequest request)
        {
            var response = await _storageService.UpdateReceiptDocumentAsync(request);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("receipts/{documentId}")]
        public async Task<IActionResult> DeleteReceiptDocument(int documentId)
        {
            var response = await _storageService.DeleteReceiptDocument(documentId);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("shipments")]
        public async Task<IActionResult> GetShipmentDocuments([FromQuery] DocumentFilterDto filters)
        {
            var ShipmentDocuments = await _storageService.GetShipmentDocumentsAsync(filters);
            return Ok(ShipmentDocuments);
        }

        [HttpPost("shipments")]
        public async Task<IActionResult> CreateShipmentDocument([FromBody] CreateShipmentDocumentRequest request)
        {
            var response = await _storageService.CreateShipmentDocumentAsync(request);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut("shipments")]
        public async Task<IActionResult> UpdateShipmentDocument([FromBody] UpdateShipmentDocumentRequest request)
        {
            var response = await _storageService.UpdateShipmentDocumentAsync(request);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("shipments/{documentId}")]
        public async Task<IActionResult> DeleteShipmentDocument(int documentId)
        {
            var response = await _storageService.DeleteShipmentDocumentAsync(documentId);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPatch("shipments/sing/{documentId}")]
        public async Task<IActionResult> SignShipmentDocument(int documentId)
        {
            var response = await _storageService.SignShipmentDocumentAsync(documentId);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPatch("shipments/withdraw/{documentId}")]
        public async Task<IActionResult> WithdrawShipmentDocument(int documentId)
        {
            var response = await _storageService.WithdrawShipmentDocumentAsync(documentId);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}