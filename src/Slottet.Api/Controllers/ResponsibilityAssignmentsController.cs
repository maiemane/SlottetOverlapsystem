using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slottet.Application.DTOs.Responsibilities;
using Slottet.Application.Interfaces;

namespace Slottet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/shifts/{shiftId:int}/responsibility-assignments")]
public sealed class ResponsibilityAssignmentsController : ControllerBase
{
    private readonly IResponsibilityAssignmentService _responsibilityAssignmentService;

    public ResponsibilityAssignmentsController(IResponsibilityAssignmentService responsibilityAssignmentService)
    {
        _responsibilityAssignmentService = responsibilityAssignmentService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Vagtansvarlig,Medarbejder")]
    public async Task<ActionResult<IReadOnlyList<ResponsibilityAssignmentDto>>> Get(int shiftId, CancellationToken cancellationToken)
    {
        var responsibilityAssignments = await _responsibilityAssignmentService.GetByShiftAsync(shiftId, cancellationToken);

        if (responsibilityAssignments is null)
        {
            return NotFound("Vagten blev ikke fundet.");
        }

        return Ok(responsibilityAssignments);
    }

    [HttpGet("{responsibilityAssignmentId:int}")]
    [Authorize(Roles = "Admin,Vagtansvarlig,Medarbejder")]
    public async Task<ActionResult<ResponsibilityAssignmentDto>> GetById(int shiftId, int responsibilityAssignmentId, CancellationToken cancellationToken)
    {
        var responsibilityAssignment = await _responsibilityAssignmentService.GetByIdAsync(shiftId, responsibilityAssignmentId, cancellationToken);

        if (responsibilityAssignment is null)
        {
            return NotFound("Ansvarstildelingen blev ikke fundet.");
        }

        return Ok(responsibilityAssignment);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Vagtansvarlig")]
    public async Task<ActionResult<ResponsibilityAssignmentDto>> Create(int shiftId, [FromBody] CreateResponsibilityAssignmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _responsibilityAssignmentService.CreateAsync(shiftId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "InvalidRequest" => BadRequest("Ansvarsomraade og medarbejder er paakraevet."),
                "ShiftNotFound" => NotFound("Vagten blev ikke fundet."),
                "ResponsibilityTypeNotFound" => NotFound("Ansvarsomraadet blev ikke fundet."),
                "EmployeeNotFound" => NotFound("Medarbejderen blev ikke fundet."),
                "EmployeeNotOnShift" => BadRequest("Medarbejderen skal vaere tilknyttet vagten foer ansvarsomraadet kan fordeles."),
                "ResponsibilityAlreadyAssigned" => BadRequest("Ansvarsomraadet er allerede tildelt paa denne vagt."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return CreatedAtAction(nameof(GetById), new { shiftId, responsibilityAssignmentId = result.ResponsibilityAssignment!.Id }, result.ResponsibilityAssignment);
    }

    [HttpPut("{responsibilityAssignmentId:int}")]
    [Authorize(Roles = "Admin,Vagtansvarlig")]
    public async Task<ActionResult<ResponsibilityAssignmentDto>> Update(int shiftId, int responsibilityAssignmentId, [FromBody] UpdateResponsibilityAssignmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _responsibilityAssignmentService.UpdateAsync(shiftId, responsibilityAssignmentId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "InvalidRequest" => BadRequest("Ansvarsomraade og medarbejder er paakraevet."),
                "ShiftNotFound" => NotFound("Vagten blev ikke fundet."),
                "ResponsibilityAssignmentNotFound" => NotFound("Ansvarstildelingen blev ikke fundet."),
                "ResponsibilityTypeNotFound" => NotFound("Ansvarsomraadet blev ikke fundet."),
                "EmployeeNotFound" => NotFound("Medarbejderen blev ikke fundet."),
                "EmployeeNotOnShift" => BadRequest("Medarbejderen skal vaere tilknyttet vagten foer ansvarsomraadet kan fordeles."),
                "ResponsibilityAlreadyAssigned" => BadRequest("Ansvarsomraadet er allerede tildelt paa denne vagt."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return Ok(result.ResponsibilityAssignment);
    }

    [HttpDelete("{responsibilityAssignmentId:int}")]
    [Authorize(Roles = "Admin,Vagtansvarlig")]
    public async Task<IActionResult> Delete(int shiftId, int responsibilityAssignmentId, CancellationToken cancellationToken)
    {
        var result = await _responsibilityAssignmentService.DeleteAsync(shiftId, responsibilityAssignmentId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "ShiftNotFound" => NotFound("Vagten blev ikke fundet."),
                "ResponsibilityAssignmentNotFound" => NotFound("Ansvarstildelingen blev ikke fundet."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return NoContent();
    }
}
