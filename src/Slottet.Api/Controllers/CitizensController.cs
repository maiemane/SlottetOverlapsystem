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

    public CitizensController(ICreateCitizenService createCitizenService)
    {
        _createCitizenService = createCitizenService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CitizenDto>>> GetAll(CancellationToken cancellationToken)
    {
        var citizens = await _createCitizenService.GetAllAsync(cancellationToken);
        return Ok(citizens);
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
            new { id = result.Citizen!.Id },
            result.Citizen);
    }

    [HttpPut("{citizenId:int}")]
    public async Task<ActionResult<CitizenDto>> Update(
        int citizenId,
        [FromBody] UpdateCitizenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createCitizenService.UpdateAsync(citizenId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "NotFound" => NotFound("Borgeren blev ikke fundet."),
                "InvalidRequest" => BadRequest("Navn, lejlighedsnummer, afdeling og trafiklys er påkrævet."),
                "DepartmentNotFound" => NotFound("Afdelingen blev ikke fundet."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return Ok(result.Citizen);
    }

    [HttpDelete("{citizenId:int}")]
    public async Task<IActionResult> Delete(int citizenId, CancellationToken cancellationToken)
    {
        var result = await _createCitizenService.DeleteAsync(citizenId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "NotFound" => NotFound("Borgeren blev ikke fundet."),
                "HasRelations" => Conflict("Borgeren kan ikke slettes, fordi den bruges i eksisterende registreringer."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return NoContent();
    }
}
