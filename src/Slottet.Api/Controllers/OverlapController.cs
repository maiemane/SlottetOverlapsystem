using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slottet.Application.DTOs.Overlap;
using Slottet.Application.Interfaces;

namespace Slottet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/overlap")]
public sealed class OverlapController : ControllerBase
{
    private readonly IOverlapOverviewService _overlapOverviewService;

    public OverlapController(IOverlapOverviewService overlapOverviewService)
    {
        _overlapOverviewService = overlapOverviewService;
    }
    
    [HttpGet("departments/{departmentId:int}/shifts/{shiftId:int}/citizens")]
    public async Task<ActionResult<CitizenOverlapOverviewDto>> GetCitizenOverview(
        int departmentId,
        int shiftId,
        CancellationToken cancellationToken)
    {
        var overview = await _overlapOverviewService.GetCitizenOverviewAsync(departmentId, shiftId, cancellationToken);

        if (overview is null)
        {
            return NotFound("Afdeling eller vagt blev ikke fundet, eller vagten tilhoerer ikke afdelingen.");
        }

        return Ok(overview);
    }
    
}
