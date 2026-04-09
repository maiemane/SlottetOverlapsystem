using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slottet.Application.DTOs.Staffing;
using Slottet.Application.Interfaces;
using Slottet.Domain.Enums;

namespace Slottet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/shifts")]
public sealed class ShiftsController : ControllerBase
{
    private readonly IStaffAllocationService _staffAllocationService;

    public ShiftsController(IStaffAllocationService staffAllocationService)
    {
        _staffAllocationService = staffAllocationService;
    }

    [HttpGet("resolve")]
    public async Task<ActionResult<ShiftLookupDto>> ResolveShift(
        [FromQuery] int departmentId,
        [FromQuery] DateTime date,
        [FromQuery] ShiftType shiftType,
        CancellationToken cancellationToken)
    {
        var shift = await _staffAllocationService.GetShiftByDepartmentDateAndTypeAsync(
            departmentId,
            date,
            shiftType,
            cancellationToken);

        if (shift is null)
        {
            return NotFound("Afdeling eller vagt blev ikke fundet.");
        }

        return Ok(shift);
    }
}
