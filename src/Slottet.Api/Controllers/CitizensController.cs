using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slottet.Application.DTOs.Citizens;
using Slottet.Application.Interfaces;

namespace Slottet.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public sealed class CitizensController : ControllerBase
{
    private readonly ICreateCitizenService _createCitizenService;
    private readonly ICreateCitizenFixedMedicationService _createCitizenFixedMedicationService;
    private readonly ICitizenFixedMedicationService _citizenFixedMedicationService;

    public CitizensController(
        ICreateCitizenService createCitizenService,
        ICreateCitizenFixedMedicationService createCitizenFixedMedicationService,
        ICitizenFixedMedicationService citizenFixedMedicationService)
    {
        _createCitizenService = createCitizenService;
        _createCitizenFixedMedicationService = createCitizenFixedMedicationService;
        _citizenFixedMedicationService = citizenFixedMedicationService;
    }
    
    [HttpPost]
    public async Task<ActionResult<CreateCitizenResponse>> Create(
        [FromBody] CreateCitizenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createCitizenService.CreateAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "InvalidRequest" => BadRequest("Navn, lejlighedsnummer, afdeling og trafiklys er påkrævet."),
                "DepartmentNotFound" => NotFound("Afdelingen blev ikke fundet."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return CreatedAtAction(
            nameof(Create),
            new { id = result.Citizen!.CitizenId },
            result.Citizen);
    }
    
    [HttpPost("{citizenId:int}/fixed-medications")]
    public async Task<ActionResult<CreateCitizenFixedMedicationResponse>> CreateFixedMedication(
        int citizenId,
        [FromBody] CreateCitizenFixedMedicationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createCitizenFixedMedicationService.CreateAsync(citizenId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "InvalidRequest" => BadRequest("Navn og gyldig vagttype er paakraevet."),
                "CitizenNotFound" => NotFound("Borgeren blev ikke fundet."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return CreatedAtAction(
            nameof(CreateFixedMedication),
            new { citizenId, id = result.FixedMedication!.FixedMedicationId },
            result.FixedMedication);
    }

    [HttpGet("{citizenId:int}/fixed-medications")]
    public async Task<ActionResult<IReadOnlyList<CitizenFixedMedicationDto>>> GetFixedMedications(
        int citizenId,
        CancellationToken cancellationToken)
    {
        var medications = await _citizenFixedMedicationService.GetByCitizenAsync(citizenId, cancellationToken);

        if (medications is null)
        {
            return NotFound("Borgeren blev ikke fundet.");
        }

        return Ok(medications);
    }

    [HttpGet("{citizenId:int}/fixed-medications/{fixedMedicationId:int}")]
    public async Task<ActionResult<CitizenFixedMedicationDto>> GetFixedMedication(
        int citizenId,
        int fixedMedicationId,
        CancellationToken cancellationToken)
    {
        var medication = await _citizenFixedMedicationService.GetByIdAsync(citizenId, fixedMedicationId, cancellationToken);

        if (medication is null)
        {
            return NotFound("Fast medicin-planen blev ikke fundet.");
        }

        return Ok(medication);
    }

    [HttpPut("{citizenId:int}/fixed-medications/{fixedMedicationId:int}")]
    public async Task<ActionResult<CitizenFixedMedicationDto>> UpdateFixedMedication(
        int citizenId,
        int fixedMedicationId,
        [FromBody] UpdateCitizenFixedMedicationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _citizenFixedMedicationService.UpdateAsync(citizenId, fixedMedicationId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "InvalidRequest" => BadRequest("Navn og gyldig vagttype er paakraevet."),
                "FixedMedicationNotFound" => NotFound("Fast medicin-planen blev ikke fundet."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return Ok(result.FixedMedication);
    }
}
