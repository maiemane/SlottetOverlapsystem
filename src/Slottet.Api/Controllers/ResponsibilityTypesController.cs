using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slottet.Application.DTOs.Responsibilities;
using Slottet.Application.Interfaces;

namespace Slottet.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Vagtansvarlig")]
[Route("api/responsibility-types")]
public sealed class ResponsibilityTypesController : ControllerBase
{
    private readonly IResponsibilityTypeService _responsibilityTypeService;

    public ResponsibilityTypesController(IResponsibilityTypeService responsibilityTypeService)
    {
        _responsibilityTypeService = responsibilityTypeService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ResponsibilityTypeDto>>> Get(CancellationToken cancellationToken)
    {
        var responsibilityTypes = await _responsibilityTypeService.GetAllAsync(cancellationToken);
        return Ok(responsibilityTypes);
    }

    [HttpGet("{responsibilityTypeId:int}")]
    public async Task<ActionResult<ResponsibilityTypeDto>> GetById(int responsibilityTypeId, CancellationToken cancellationToken)
    {
        var responsibilityType = await _responsibilityTypeService.GetByIdAsync(responsibilityTypeId, cancellationToken);

        if (responsibilityType is null)
        {
            return NotFound("Ansvarsomraadet blev ikke fundet.");
        }

        return Ok(responsibilityType);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponsibilityTypeDto>> Create([FromBody] CreateResponsibilityTypeRequest request, CancellationToken cancellationToken)
    {
        var result = await _responsibilityTypeService.CreateAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "InvalidRequest" => BadRequest("Navn er paakraevet."),
                "ResponsibilityTypeAlreadyExists" => BadRequest("Ansvarsomraadet findes allerede."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return CreatedAtAction(nameof(GetById), new { responsibilityTypeId = result.ResponsibilityType!.Id }, result.ResponsibilityType);
    }

    [HttpPut("{responsibilityTypeId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ResponsibilityTypeDto>> Update(int responsibilityTypeId, [FromBody] UpdateResponsibilityTypeRequest request, CancellationToken cancellationToken)
    {
        var result = await _responsibilityTypeService.UpdateAsync(responsibilityTypeId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "InvalidRequest" => BadRequest("Navn er paakraevet."),
                "ResponsibilityTypeNotFound" => NotFound("Ansvarsomraadet blev ikke fundet."),
                "ResponsibilityTypeAlreadyExists" => BadRequest("Ansvarsomraadet findes allerede."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return Ok(result.ResponsibilityType);
    }

    [HttpDelete("{responsibilityTypeId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int responsibilityTypeId, CancellationToken cancellationToken)
    {
        var result = await _responsibilityTypeService.DeleteAsync(responsibilityTypeId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "ResponsibilityTypeNotFound" => NotFound("Ansvarsomraadet blev ikke fundet."),
                "ResponsibilityTypeInUse" => BadRequest("Ansvarsomraadet kan ikke slettes, fordi det er tildelt en vagt."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return NoContent();
    }
}
