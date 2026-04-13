using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slottet.Application.DTOs.PhoneAllocations;
using Slottet.Application.Interfaces;

namespace Slottet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/shifts/{shiftId:int}/phone-allocations")]
public sealed class PhoneAllocationsController : ControllerBase
{
    private readonly IPhoneAllocationService _phoneAllocationService;

    public PhoneAllocationsController(IPhoneAllocationService phoneAllocationService)
    {
        _phoneAllocationService = phoneAllocationService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Vagtansvarlig,Medarbejder")]
    public async Task<ActionResult<IReadOnlyList<PhoneAllocationDto>>> Get(int shiftId, CancellationToken cancellationToken)
    {
        var phoneAllocations = await _phoneAllocationService.GetByShiftAsync(shiftId, cancellationToken);

        if (phoneAllocations is null)
        {
            return NotFound("Vagten blev ikke fundet.");
        }

        return Ok(phoneAllocations);
    }

    [HttpGet("{phoneAllocationId:int}")]
    [Authorize(Roles = "Admin,Vagtansvarlig,Medarbejder")]
    public async Task<ActionResult<PhoneAllocationDto>> GetById(int shiftId, int phoneAllocationId, CancellationToken cancellationToken)
    {
        var phoneAllocation = await _phoneAllocationService.GetByIdAsync(shiftId, phoneAllocationId, cancellationToken);

        if (phoneAllocation is null)
        {
            return NotFound("Telefontildelingen blev ikke fundet.");
        }

        return Ok(phoneAllocation);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Vagtansvarlig")]
    public async Task<ActionResult<PhoneAllocationDto>> Create(int shiftId, [FromBody] CreatePhoneAllocationRequest request, CancellationToken cancellationToken)
    {
        var result = await _phoneAllocationService.CreateAsync(shiftId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "InvalidRequest" => BadRequest("Telefon og medarbejder er paakraevet."),
                "ShiftNotFound" => NotFound("Vagten blev ikke fundet."),
                "PhoneNotFound" => NotFound("Telefonen blev ikke fundet."),
                "EmployeeNotFound" => NotFound("Medarbejderen blev ikke fundet."),
                "EmployeeNotOnShift" => BadRequest("Medarbejderen skal vaere tilknyttet vagten foer telefonen kan fordeles."),
                "PhoneAlreadyAssigned" => BadRequest("Telefonen er allerede tildelt paa denne vagt."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return CreatedAtAction(nameof(GetById), new { shiftId, phoneAllocationId = result.PhoneAllocation!.Id }, result.PhoneAllocation);
    }

    [HttpPut("{phoneAllocationId:int}")]
    [Authorize(Roles = "Admin,Vagtansvarlig")]
    public async Task<ActionResult<PhoneAllocationDto>> Update(int shiftId, int phoneAllocationId, [FromBody] UpdatePhoneAllocationRequest request, CancellationToken cancellationToken)
    {
        var result = await _phoneAllocationService.UpdateAsync(shiftId, phoneAllocationId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "InvalidRequest" => BadRequest("Telefon og medarbejder er paakraevet."),
                "ShiftNotFound" => NotFound("Vagten blev ikke fundet."),
                "PhoneAllocationNotFound" => NotFound("Telefontildelingen blev ikke fundet."),
                "PhoneNotFound" => NotFound("Telefonen blev ikke fundet."),
                "EmployeeNotFound" => NotFound("Medarbejderen blev ikke fundet."),
                "EmployeeNotOnShift" => BadRequest("Medarbejderen skal vaere tilknyttet vagten foer telefonen kan fordeles."),
                "PhoneAlreadyAssigned" => BadRequest("Telefonen er allerede tildelt paa denne vagt."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return Ok(result.PhoneAllocation);
    }

    [HttpDelete("{phoneAllocationId:int}")]
    [Authorize(Roles = "Admin,Vagtansvarlig")]
    public async Task<IActionResult> Delete(int shiftId, int phoneAllocationId, CancellationToken cancellationToken)
    {
        var result = await _phoneAllocationService.DeleteAsync(shiftId, phoneAllocationId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "ShiftNotFound" => NotFound("Vagten blev ikke fundet."),
                "PhoneAllocationNotFound" => NotFound("Telefontildelingen blev ikke fundet."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return NoContent();
    }
}
