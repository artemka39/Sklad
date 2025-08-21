using Microsoft.AspNetCore.Mvc;
using Sklad.Application.Interfaces;
using Sklad.Application.Services;
using Sklad.Contracts.Dtos;
using Sklad.Contracts.Requests;

namespace Sklad.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentsController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;
        public ShipmentsController(IShipmentService shipmentService)
        {
            _shipmentService = shipmentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetShipmentDocuments([FromQuery] DocumentFilterDto filters)
        {
            var shipmentDocuments = await _shipmentService.GetShipmentDocumentsAsync(filters);
            return Ok(shipmentDocuments);
        }

        [HttpPost]
        public async Task<IActionResult> CreateShipmentDocument([FromBody] CreateShipmentDocumentRequest request)
        {
            var response = await _shipmentService.CreateShipmentDocumentAsync(request);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateShipmentDocument([FromBody] UpdateShipmentDocumentRequest request)
        {
            var response = await _shipmentService.UpdateShipmentDocumentAsync(request);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("{documentId}")]
        public async Task<IActionResult> DeleteShipmentDocument(int documentId)
        {
            var response = await _shipmentService.DeleteShipmentDocumentAsync(documentId);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPatch("sing/{documentId}")]
        public async Task<IActionResult> SignShipmentDocument(int documentId)
        {
            var response = await _shipmentService.SignShipmentDocumentAsync(documentId);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPatch("withdraw/{documentId}")]
        public async Task<IActionResult> WithdrawShipmentDocument(int documentId)
        {
            var response = await _shipmentService.WithdrawShipmentDocumentAsync(documentId);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
