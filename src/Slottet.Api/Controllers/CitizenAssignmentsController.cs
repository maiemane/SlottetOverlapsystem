using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slottet.Application.DTOs.Staffing;
using Slottet.Application.Interfaces;

namespace Slottet.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Vagtansvarlig")]
[Route("api/citizen-assignments")]
public sealed class CitizenAssignmentsController : ControllerBase
{
    private readonly IStaffAllocationService _staffAllocationService;

    public CitizenAssignmentsController(IStaffAllocationService staffAllocationService)
    {
        _staffAllocationService = staffAllocationService;
    }

    [HttpGet("board")]
    public async Task<ActionResult<CitizenAssignmentBoardDto>> GetBoard(
        [FromQuery] int departmentId,
        [FromQuery] DateTime date,
        CancellationToken cancellationToken)
    {
        var board = await _staffAllocationService.GetCitizenAssignmentBoardAsync(departmentId, date, cancellationToken);

        if (board is null)
        {
            return NotFound("Afdelingen blev ikke fundet.");
        }

        return Ok(board);
    }
}
