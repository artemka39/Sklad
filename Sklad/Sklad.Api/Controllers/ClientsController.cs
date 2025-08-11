using Microsoft.AspNetCore.Mvc;
using Sklad.Application.Interfaces;
using Sklad.Domain.Enums;
using Sklad.Domain.Models;

namespace Sklad.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;
        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }
        [HttpGet]
        public async Task<IActionResult> GetClients([FromQuery] CatalogEntityStateEnum? state)
        {
            var clients = await _clientService.GetClientsAsync(state);
            return Ok(clients);
        }

        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] Client client)
        {
            var response = await _clientService.CreateClientAsync(client);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateClient([FromBody] Client client)
        {
            var response = await _clientService.UpdateClientAsync(client);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var response = await _clientService.DeleteClientAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteMultipleClients([FromBody] int[] ids)
        {
            var response = await _clientService.DeleteMultipleClientsAsync(ids);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("archive")]
        public async Task<IActionResult> ArchiveMultipleClients([FromBody] Client[] clients)
        {
            var response = await _clientService.ArchiveMultipleClientsAsync(clients);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
