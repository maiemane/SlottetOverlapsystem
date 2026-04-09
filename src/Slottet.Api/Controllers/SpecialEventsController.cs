using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slottet.Application.DTOs.SpecialEvents;
using Slottet.Application.Interfaces;

namespace Slottet.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Medarbejder")]
[Route("api/shifts/{shiftId:int}/citizens/{citizenId:int}/special-events")]
public sealed class SpecialEventsController : ControllerBase
{
    private readonly ISpecialEventService _specialEventService;

    public SpecialEventsController(ISpecialEventService specialEventService)
    {
        _specialEventService = specialEventService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SpecialEventDto>>> Get(
        int shiftId,
        int citizenId,
        CancellationToken cancellationToken)
    {
        var specialEvents = await _specialEventService.GetByCitizenAndShiftAsync(shiftId, citizenId, cancellationToken);

        if (specialEvents is null)
        {
            return NotFound("Vagten eller borgeren blev ikke fundet.");
        }

        return Ok(specialEvents);
    }

    [HttpGet("{specialEventId:int}")]
    public async Task<ActionResult<SpecialEventDto>> GetById(
        int shiftId,
        int citizenId,
        int specialEventId,
        CancellationToken cancellationToken)
    {
        var specialEvent = await _specialEventService.GetByIdAsync(shiftId, citizenId, specialEventId, cancellationToken);

        if (specialEvent is null)
        {
            return NotFound("Den saerlige haendelse blev ikke fundet.");
        }

        return Ok(specialEvent);
    }

    [HttpPost]
    public async Task<ActionResult<SpecialEventDto>> Create(
        int shiftId,
        int citizenId,
        [FromBody] CreateSpecialEventRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _specialEventService.CreateAsync(shiftId, citizenId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "InvalidRequest" => BadRequest("Beskrivelse og tidspunkt er paakraevet."),
                "ShiftNotFound" => NotFound("Vagten blev ikke fundet."),
                "CitizenNotFound" => NotFound("Borgeren blev ikke fundet."),
                "CitizenNotInShiftDepartment" => BadRequest("Borgeren tilhoerer ikke den valgte vagts afdeling."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return CreatedAtAction(
            nameof(GetById),
            new { shiftId, citizenId, specialEventId = result.SpecialEvent!.Id },
            result.SpecialEvent);
    }

    [HttpPut("{specialEventId:int}")]
    public async Task<ActionResult<SpecialEventDto>> Update(
        int shiftId,
        int citizenId,
        int specialEventId,
        [FromBody] UpdateSpecialEventRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _specialEventService.UpdateAsync(shiftId, citizenId, specialEventId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "InvalidRequest" => BadRequest("Beskrivelse og tidspunkt er paakraevet."),
                "ShiftNotFound" => NotFound("Vagten blev ikke fundet."),
                "CitizenNotFound" => NotFound("Borgeren blev ikke fundet."),
                "CitizenNotInShiftDepartment" => BadRequest("Borgeren tilhoerer ikke den valgte vagts afdeling."),
                "SpecialEventNotFound" => NotFound("Den saerlige haendelse blev ikke fundet."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return Ok(result.SpecialEvent);
    }

    [HttpDelete("{specialEventId:int}")]
    public async Task<IActionResult> Delete(
        int shiftId,
        int citizenId,
        int specialEventId,
        CancellationToken cancellationToken)
    {
        var result = await _specialEventService.DeleteAsync(shiftId, citizenId, specialEventId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "ShiftNotFound" => NotFound("Vagten blev ikke fundet."),
                "CitizenNotFound" => NotFound("Borgeren blev ikke fundet."),
                "CitizenNotInShiftDepartment" => BadRequest("Borgeren tilhoerer ikke den valgte vagts afdeling."),
                "SpecialEventNotFound" => NotFound("Den saerlige haendelse blev ikke fundet."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return NoContent();
    }
}
