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
    }
}