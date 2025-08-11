using Microsoft.AspNetCore.Mvc;
using Sklad.Application.Interfaces;
using Sklad.Domain.Enums;
using Sklad.Domain.Models;

namespace Sklad.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnitsController : ControllerBase
    {
        private readonly IUnitService _unitService;
        public UnitsController(IUnitService unitService)
        {
            _unitService = unitService;
        }
        [HttpGet]
        public async Task<IActionResult> GetUnits([FromQuery] CatalogEntityStateEnum? state)
        {
            var units = await _unitService.GetUnitsAsync(state);
            return Ok(units);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUnit([FromBody] Unit unit)
        {
            var response = await _unitService.CreateUnitAsync(unit);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUnit([FromBody] Unit unit)
        {
            var response = await _unitService.UpdateUnitAsync(unit);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUnit(int id)
        {
            var response = await _unitService.DeleteUnitAsync(id);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteMultipleUnits([FromBody] int[] ids)
        {
            var response = await _unitService.DeleteMultipleUnitsAsync(ids);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("archive")]
        public async Task<IActionResult> ArchiveMultipleUnits([FromBody] Unit[] units)
        {
            var response = await _unitService.ArchiveMultipleUnitsAsync(units);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
