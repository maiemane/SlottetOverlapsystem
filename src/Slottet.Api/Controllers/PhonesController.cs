using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slottet.Application.DTOs.Phones;
using Slottet.Application.Interfaces;

namespace Slottet.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Vagtansvarlig")]
[Route("api/phones")]
public sealed class PhonesController : ControllerBase
{
    private readonly IPhoneService _phoneService;

    public PhonesController(IPhoneService phoneService)
    {
        _phoneService = phoneService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PhoneDto>>> Get(CancellationToken cancellationToken)
    {
        var phones = await _phoneService.GetAllAsync(cancellationToken);
        return Ok(phones);
    }

    [HttpGet("{phoneId:int}")]
    public async Task<ActionResult<PhoneDto>> GetById(int phoneId, CancellationToken cancellationToken)
    {
        var phone = await _phoneService.GetByIdAsync(phoneId, cancellationToken);

        if (phone is null)
        {
            return NotFound("Telefonen blev ikke fundet.");
        }

        return Ok(phone);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PhoneDto>> Create([FromBody] CreatePhoneRequest request, CancellationToken cancellationToken)
    {
        var result = await _phoneService.CreateAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "InvalidRequest" => BadRequest("Navn eller nummer er paakraevet."),
                "PhoneAlreadyExists" => BadRequest("Telefonen findes allerede."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return CreatedAtAction(nameof(GetById), new { phoneId = result.Phone!.Id }, result.Phone);
    }

    [HttpPut("{phoneId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PhoneDto>> Update(int phoneId, [FromBody] UpdatePhoneRequest request, CancellationToken cancellationToken)
    {
        var result = await _phoneService.UpdateAsync(phoneId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "InvalidRequest" => BadRequest("Navn eller nummer er paakraevet."),
                "PhoneNotFound" => NotFound("Telefonen blev ikke fundet."),
                "PhoneAlreadyExists" => BadRequest("Telefonen findes allerede."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return Ok(result.Phone);
    }

    [HttpDelete("{phoneId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int phoneId, CancellationToken cancellationToken)
    {
        var result = await _phoneService.DeleteAsync(phoneId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "PhoneNotFound" => NotFound("Telefonen blev ikke fundet."),
                "PhoneInUse" => BadRequest("Telefonen kan ikke slettes, fordi den er tildelt en vagt."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return NoContent();
    }
}
