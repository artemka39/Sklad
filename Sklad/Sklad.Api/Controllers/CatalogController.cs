using Azure;
using Microsoft.AspNetCore.Mvc;
using Sklad.Domain.Interfaces;
using Sklad.Domain.Models;
using System.Threading.Tasks;

namespace Sklad.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly ICatalogService _catalogService;

        public CatalogController(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        [HttpGet("resources")]
        public async Task<IActionResult> GetResources()
        {
            var resources = await _catalogService.GetResourcesAsync();
            return Ok(resources);
        }

        [HttpPost("resource")]
        public async Task<IActionResult> CreateResource([FromBody] Resource resource)
        {
            var response = await _catalogService.CreateResourceAsync(resource);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut("resource")]
        public async Task<IActionResult> UpdateResource([FromBody] Resource resource)
        {
            var response = await _catalogService.UpdateResourceAsync(resource);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("resource/{id}")]
        public async Task<IActionResult> DeleteResource(int id)
        {
            var response = await _catalogService.DeleteResourceAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPatch("resource")]
        public async Task<IActionResult> ArchiveResource([FromBody] Resource resource)
        {
            var response = await _catalogService.ArchiveResourceAsync(resource);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("units")]
        public async Task<IActionResult> GetUnitOfMeasurements()
        {
            var units = await _catalogService.GetUnitsOfMeasurementAsync();
            return Ok(units);
        }

        [HttpPost("unit")]
        public async Task<IActionResult> CreateUnitOfMeasurement([FromBody] UnitOfMeasurement unit)
        {
            var response = await _catalogService.CreateUnitOfMeasurementAsync(unit);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut("unit")]
        public async Task<IActionResult> UpdateUnitOfMeasurement([FromBody] UnitOfMeasurement unit)
        {
            var response = await _catalogService.UpdateUnitOfMeasurementAsync(unit);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("unit/{id}")]
        public async Task<IActionResult> DeleteUnitOfMeasurement(int id)
        {
            var response = await _catalogService.DeleteUnitOfMeasurementAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPatch("unit")]
        public async Task<IActionResult> ArchiveUnitOfMeasurement([FromBody] UnitOfMeasurement unit)
        {
            var response = await _catalogService.ArchiveUnitOfMeasurementAsync(unit);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("clients")]
        public async Task<IActionResult> GetClients()
        {
            var clients = await _catalogService.GetClientsAsync();
            return Ok(clients);
        }

        [HttpPost("client")]
        public async Task<IActionResult> CreateClient([FromBody] Client client)
        {
            var response = await _catalogService.CreateClientAsync(client);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut("client")]
        public async Task<IActionResult> UpdateClient([FromBody] Client client)
        {
            var response = await _catalogService.UpdateClientAsync(client);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("client/{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var response = await _catalogService.DeleteClientAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPatch("client")]
        public async Task<IActionResult> ArchiveClient([FromBody] Client client)
        {
            var response = await _catalogService.ArchiveClientAsync(client);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}