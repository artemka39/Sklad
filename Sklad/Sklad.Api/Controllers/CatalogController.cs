using Azure;
using Microsoft.AspNetCore.Mvc;
using Sklad.Application.Interfaces;
using Sklad.Domain.Enums;
using Sklad.Domain.Models;
using System.Threading.Tasks;

namespace Sklad.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly IResourceService _resourceService;
        private readonly IUnitService _unitService;
        private readonly IClientService _clientService;

        public CatalogController(
            IResourceService resourceService,
            IUnitService unitService,
            IClientService clientService)
        {
            _resourceService = resourceService;
            _unitService = unitService;
            _clientService = clientService;
        }

        [HttpGet("resources")]
        public async Task<IActionResult> GetResources([FromQuery] CatalogEntityStateEnum? state)
        {
            var resources = await _resourceService.GetResourcesAsync(state);
            return Ok(resources);
        }

        [HttpPost("resource")]
        public async Task<IActionResult> CreateResource([FromBody] Resource resource)
        {
            var response = await _resourceService.CreateResourceAsync(resource);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut("resource")]
        public async Task<IActionResult> UpdateResource([FromBody] Resource resource)
        {
            var response = await _resourceService.UpdateResourceAsync(resource);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("resource/{id}")]
        public async Task<IActionResult> DeleteResource(int id)
        {
            var response = await _resourceService.DeleteResourceAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("resource")]
        public async Task<IActionResult> DeleteMultipleResources([FromBody] int[] resourceIds)
        {
            var response = await _resourceService.DeleteMultipleResourceAsync(resourceIds);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPatch("resource")]
        public async Task<IActionResult> ArchiveResource([FromBody] Resource resource)
        {
            var response = await _resourceService.ArchiveResourceAsync(resource);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("units")]
        public async Task<IActionResult> GetUnits([FromQuery] CatalogEntityStateEnum? state)
        {
            var units = await _unitService.GetUnitsAsync(state);
            return Ok(units);
        }

        [HttpPost("unit")]
        public async Task<IActionResult> CreateUnit([FromBody] Unit unit)
        {
            var response = await _unitService.CreateUnitAsync(unit);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut("unit")]
        public async Task<IActionResult> UpdateUnit([FromBody] Unit unit)
        {
            var response = await _unitService.UpdateUnitAsync(unit);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("unit/{id}")]
        public async Task<IActionResult> DeleteUnit(int id)
        {
            var response = await _unitService.DeleteUnitAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPatch("unit")]
        public async Task<IActionResult> ArchiveUnit([FromBody] Unit unit)
        {
            var response = await _unitService.ArchiveUnitAsync(unit);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpGet("clients")]
        public async Task<IActionResult> GetClients([FromQuery] CatalogEntityStateEnum? state)
        {
            var clients = await _clientService.GetClientsAsync(state);
            return Ok(clients);
        }

        [HttpPost("client")]
        public async Task<IActionResult> CreateClient([FromBody] Client client)
        {
            var response = await _clientService.CreateClientAsync(client);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut("client")]
        public async Task<IActionResult> UpdateClient([FromBody] Client client)
        {
            var response = await _clientService.UpdateClientAsync(client);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("client/{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var response = await _clientService.DeleteClientAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPatch("client")]
        public async Task<IActionResult> ArchiveClient([FromBody] Client client)
        {
            var response = await _clientService.ArchiveClientAsync(client);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}