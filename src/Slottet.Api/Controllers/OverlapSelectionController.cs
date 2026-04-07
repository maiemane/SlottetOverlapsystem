using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slottet.Application.DTOs;
using Slottet.Application.Interfaces;

namespace Slottet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/overlap-selection")]
public class OverlapSelectionController : ControllerBase
{
    private readonly IOverlapSelectionService _overlapSelectionService;

    public OverlapSelectionController(IOverlapSelectionService overlapSelectionService)
    {
        _overlapSelectionService = overlapSelectionService;
    }

    [HttpGet("departments/{departmentId:int}/shifts")]
    public async Task<ActionResult<IEnumerable<ShiftOptionDto>>> GetAvailableShifts(int departmentId)
    {
        var shifts = await _overlapSelectionService.GetAvailableShiftsAsync(departmentId);

        if (shifts is null)
        {
            return NotFound($"Afdeling med id {departmentId} blev ikke fundet.");
        }

        return Ok(shifts);
    }

    [HttpPost("confirm")]
    public async Task<ActionResult<SelectOverlapResponseDto>> ConfirmSelection(
        [FromBody] SelectOverlapRequestDto request)
    {
        var result = await _overlapSelectionService.SelectOverlapAsync(request);

        if (result is null)
        {
            return BadRequest("Ugyldigt valg af afdeling eller vagt.");
        }

        return Ok(result);
    }
}
