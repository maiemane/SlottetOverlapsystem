using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slottet.Application.DTOs.Medications;
using Slottet.Application.Interfaces;

namespace Slottet.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Medarbejder")]
[Route("api/shifts/{shiftId:int}/citizens/{citizenId:int}/medication-registrations")]
public sealed class MedicationRegistrationsController : ControllerBase
{
    private readonly IMedicationRegistrationService _medicationRegistrationService;

    public MedicationRegistrationsController(IMedicationRegistrationService medicationRegistrationService)
    {
        _medicationRegistrationService = medicationRegistrationService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MedicationRegistrationDto>>> Get(
        int shiftId,
        int citizenId,
        CancellationToken cancellationToken)
    {
        var registrations = await _medicationRegistrationService.GetByCitizenAndShiftAsync(shiftId, citizenId, cancellationToken);

        if (registrations is null)
        {
            return NotFound("Vagten eller borgeren blev ikke fundet.");
        }

        return Ok(registrations);
    }

    [HttpPost]
    public async Task<ActionResult<MedicationRegistrationDto>> Create(
        int shiftId,
        int citizenId,
        [FromBody] CreateMedicationRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _medicationRegistrationService.CreateAsync(shiftId, citizenId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "InvalidRequest" => BadRequest("Requesten er ugyldig."),
                "ShiftNotFound" => NotFound("Vagten blev ikke fundet."),
                "CitizenNotFound" => NotFound("Borgeren blev ikke fundet."),
                "CitizenNotInShiftDepartment" => BadRequest("Borgeren tilhoerer ikke den valgte vagts afdeling."),
                "FixedMedicationNotFound" => NotFound("Den faste medicin-plan blev ikke fundet for borgeren paa denne vagttype."),
                "AlreadyRegistered" => BadRequest("Den faste medicin er allerede registreret paa denne vagt."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return CreatedAtAction(nameof(Create), new { shiftId, citizenId, id = result.Registration!.Id }, result.Registration);
    }

    [HttpDelete("{medicationRegistrationId:int}")]
    public async Task<IActionResult> Delete(
        int shiftId,
        int citizenId,
        int medicationRegistrationId,
        CancellationToken cancellationToken)
    {
        var result = await _medicationRegistrationService.DeleteAsync(shiftId, citizenId, medicationRegistrationId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "InvalidRequest" => BadRequest("Requesten er ugyldig."),
                "ShiftNotFound" => NotFound("Vagten blev ikke fundet."),
                "CitizenNotFound" => NotFound("Borgeren blev ikke fundet."),
                "CitizenNotInShiftDepartment" => BadRequest("Borgeren tilhoerer ikke den valgte vagts afdeling."),
                "RegistrationNotFound" => NotFound("Medicinregistreringen blev ikke fundet."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return NoContent();
    }
}
