using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slottet.Application.DTOs.ShiftDefinitions;
using Slottet.Application.Interfaces;

namespace Slottet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/shift-definitions")]
public sealed class ShiftDefinitionsController : ControllerBase
{
    private readonly IShiftDefinitionService _shiftDefinitionService;

    public ShiftDefinitionsController(IShiftDefinitionService shiftDefinitionService)
    {
        _shiftDefinitionService = shiftDefinitionService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ShiftDefinitionDto>>> GetAll(CancellationToken cancellationToken)
    {
        var definitions = await _shiftDefinitionService.GetAllAsync(cancellationToken);
        return Ok(definitions);
    }

    [HttpGet("{shiftDefinitionId:int}")]
    public async Task<ActionResult<ShiftDefinitionDto>> GetById(int shiftDefinitionId, CancellationToken cancellationToken)
    {
        var definition = await _shiftDefinitionService.GetByIdAsync(shiftDefinitionId, cancellationToken);

        if (definition is null)
        {
            return NotFound("Vagtdefinitionen blev ikke fundet.");
        }

        return Ok(definition);
    }

    [HttpGet("resolve")]
    public async Task<ActionResult<ResolvedShiftTypeDto>> Resolve([FromQuery] TimeOnly time, CancellationToken cancellationToken)
    {
        var resolved = await _shiftDefinitionService.ResolveByTimeAsync(time, cancellationToken);

        if (resolved is null)
        {
            return NotFound("Ingen aktiv vagtdefinition matcher tidspunktet.");
        }

        return Ok(resolved);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{shiftDefinitionId:int}")]
    public async Task<ActionResult<ShiftDefinitionDto>> Update(
        int shiftDefinitionId,
        [FromBody] UpdateShiftDefinitionRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _shiftDefinitionService.UpdateAsync(shiftDefinitionId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "InvalidRequest" => BadRequest("ShiftType, starttid og sluttid skal vaere gyldige."),
                "NotFound" => NotFound("Vagtdefinitionen blev ikke fundet."),
                "ShiftTypeMismatch" => BadRequest("ShiftType kan ikke aendres for en eksisterende vagtdefinition."),
                "InvalidSchedule" => BadRequest("Vagtdefinitionerne overlapper eller er ugyldige."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return Ok(result.ShiftDefinition);
    }
}
