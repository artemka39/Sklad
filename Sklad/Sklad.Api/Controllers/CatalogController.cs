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

        [HttpGet("units")]
        public async Task<IActionResult> GetUnitOfMeasurements()
        {
            var units = await _catalogService.GetUnitOfMeasurementsAsync();
            return Ok(units);
        }

        [HttpGet("clients")]
        public async Task<IActionResult> GetClients()
        {
            var clients = await _catalogService.GetClientsAsync();
            return Ok(clients);
        }

        [HttpPost("resource")]
        public async Task<IActionResult> CreateResource([FromBody] Resource resource)
        {
            try
            {
                await _catalogService.CreateResourceAsync(resource);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("unit")]
        public async Task<IActionResult> CreateUnitOfMeasurement([FromBody] UnitOfMeasurement unit)
        {
            try
            {
                await _catalogService.CreateUnitOfMeasurementAsync(unit);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("client")]
        public async Task<IActionResult> CreateClient([FromBody] Client client)
        {
            try
            {
                await _catalogService.CreateClientAsync(client);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("resource")]
        public async Task<IActionResult> UpdateResource([FromBody] Resource resource)
        {
            try
            {
                await _catalogService.UpdateResourceAsync(resource);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("resource/{id}")]
        public async Task<IActionResult> DeleteResource(int id)
        {
            try
            {
                await _catalogService.DeleteResourceAsync(id);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("unit")]
        public async Task<IActionResult> UpdateUnitOfMeasurement([FromBody] UnitOfMeasurement unit)
        {
            try
            {
                await _catalogService.UpdateUnitOfMeasurementAsync(unit);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("unit/{id}")]
        public async Task<IActionResult> DeleteUnitOfMeasurement(int id)
        {
            try
            {
                await _catalogService.DeleteUnitOfMeasurementAsync(id);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("client")]
        public async Task<IActionResult> UpdateClient([FromBody] Client client)
        {
            try
            {
                await _catalogService.UpdateClientAsync(client);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("client/{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            try
            {
                await _catalogService.DeleteClientAsync(id);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
