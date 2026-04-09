using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slottet.Application.DTOs.Overlap;
using Slottet.Application.DTOs.Staffing;
using Slottet.Application.Interfaces;

namespace Slottet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/overlap")]
public sealed class OverlapController : ControllerBase
{
    private readonly IOverlapOverviewService _overlapOverviewService;
    private readonly IStaffAllocationService _staffAllocationService;

    public OverlapController(IOverlapOverviewService overlapOverviewService, IStaffAllocationService staffAllocationService)
    {
        _overlapOverviewService = overlapOverviewService;
        _staffAllocationService = staffAllocationService;
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
