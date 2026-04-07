using Microsoft.AspNetCore.Mvc;
using Slottet.Application.DTOs;
using Slottet.Application.Interfaces;
using Slottet.Domain.Enums;

namespace Slottet.Api.Controllers;

[ApiController]
[Route("api/overlap")]
public class OverlapController : ControllerBase
{
    private readonly IOverlapOverviewService _overlapOverviewService;

    public OverlapController(IOverlapOverviewService overlapOverviewService)
    {
        _overlapOverviewService = overlapOverviewService;
    }

    [HttpGet("departments/{departmentId:int}/shifts/{shiftType}")]
    public async Task<ActionResult<OverlapOverviewDto>> GetOverview(int departmentId, ShiftType shiftType)
    {
        var overview = await _overlapOverviewService.GetOverviewAsync(departmentId, shiftType);

        if (overview is null)
        {
            return BadRequest("Ugyldig afdeling eller vagt.");
        }

        return Ok(overview);
    }
}