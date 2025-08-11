using Microsoft.AspNetCore.Mvc;
using Sklad.Application.Interfaces;
using Sklad.Domain.Enums;
using Sklad.Domain.Models;

namespace Sklad.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResourcesController : Controller
    {
        private readonly IResourceService _resourceService;
        public ResourcesController(IResourceService resourceService)
        {
            _resourceService = resourceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetResources([FromQuery] CatalogEntityStateEnum? state)
        {
            var resources = await _resourceService.GetResourcesAsync(state);
            return Ok(resources);
        }

        [HttpPost]
        public async Task<IActionResult> CreateResource([FromBody] Resource resource)
        {
            var response = await _resourceService.CreateResourceAsync(resource);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateResource([FromBody] Resource resource)
        {
            var response = await _resourceService.UpdateResourceAsync(resource);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResource(int id)
        {
            var response = await _resourceService.DeleteResourceAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteMultipleResources([FromBody] int[] ids)
        {
            var response = await _resourceService.DeleteMultipleResourcesAsync(ids);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("archive")]
        public async Task<IActionResult> ArchiveMultipleResources([FromBody] Resource[] resources)
        {
            var response = await _resourceService.ArchiveMultipleResourcesAsync(resources);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
