using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slottet.Application.DTOs.Staffing;
using Slottet.Application.Interfaces;

namespace Slottet.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Vagtansvarlig")]
[Route("api/shifts/{shiftId:int}")]
public sealed class ShiftStaffingController : ControllerBase
{
    private readonly IStaffAllocationService _staffAllocationService;

    public ShiftStaffingController(IStaffAllocationService staffAllocationService)
    {
        _staffAllocationService = staffAllocationService;
    }

    [HttpGet("employees")]
    public async Task<ActionResult<ShiftStaffingDto>> GetShiftEmployees(
        int shiftId,
        CancellationToken cancellationToken)
    {
        var staffing = await _staffAllocationService.GetShiftEmployeesAsync(shiftId, cancellationToken);

        if (staffing is null)
        {
            return NotFound("Vagten blev ikke fundet.");
        }

        return Ok(staffing);
    }

    [HttpPut("employees")]
    public async Task<ActionResult<AssignEmployeesToShiftResponse>> AssignEmployeesToShift(
        int shiftId,
        [FromBody] AssignEmployeesToShiftRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _staffAllocationService.AssignEmployeesToShiftAsync(shiftId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "InvalidRequest" => BadRequest("Requesten er ugyldig."),
                "ShiftNotFound" => NotFound("Vagten blev ikke fundet."),
                "EmployeeNotFound" => BadRequest("En eller flere medarbejdere blev ikke fundet eller er ikke aktive."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return Ok(result.Assignment);
    }

    [HttpGet("citizens/{citizenId:int}/employees")]
    public async Task<ActionResult<CitizenStaffingDto>> GetCitizenEmployees(
        int shiftId,
        int citizenId,
        CancellationToken cancellationToken)
    {
        var staffing = await _staffAllocationService.GetCitizenEmployeesAsync(shiftId, citizenId, cancellationToken);

        if (staffing is null)
        {
            return NotFound("Vagten eller borgeren blev ikke fundet, eller borgeren tilhoerer ikke den valgte afdelings vagt.");
        }

        return Ok(staffing);
    }

    [HttpPut("citizens/{citizenId:int}/employees")]
    public async Task<ActionResult<AssignEmployeesToCitizenResponse>> AssignEmployeesToCitizen(
        int shiftId,
        int citizenId,
        [FromBody] AssignEmployeesToCitizenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _staffAllocationService.AssignEmployeesToCitizenAsync(shiftId, citizenId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "InvalidRequest" => BadRequest("Requesten er ugyldig."),
                "ShiftNotFound" => NotFound("Vagten blev ikke fundet."),
                "CitizenNotFound" => NotFound("Borgeren blev ikke fundet."),
                "CitizenNotInShiftDepartment" => BadRequest("Borgeren tilhoerer ikke den valgte vagts afdeling."),
                "TooManyEmployees" => BadRequest("En borger kan hoejst have to medarbejdere tilknyttet."),
                "EmployeeNotOnShift" => BadRequest("Alle medarbejdere skal vaere tilknyttet vagten, foer de kan fordeles til en borger."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return Ok(result.Assignment);
    }
}
