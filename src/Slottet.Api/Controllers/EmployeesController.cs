using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slottet.Application.DTOs.Employees;
using Slottet.Application.Interfaces;

namespace Slottet.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public sealed class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EmployeeDto>>> GetAll(CancellationToken cancellationToken)
    {
        var employees = await _employeeService.GetAllAsync(cancellationToken);
        return Ok(employees);
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _employeeService.CreateAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "MissingFields" => BadRequest("Navn, email, password og rolle er påkrævet."),
                "InvalidRole" => BadRequest("Rollen er ugyldig."),
                "PasswordTooShort" => BadRequest("Password skal være mindst 6 tegn."),
                "EmailAlreadyExists" => Conflict("Der findes allerede en medarbejder med den email."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return Created($"api/employees/{result.Employee!.Id}", result.Employee);
    }

    [HttpDelete("{employeeId:int}")]
    public async Task<IActionResult> Delete(int employeeId, CancellationToken cancellationToken)
    {
        var result = await _employeeService.DeleteAsync(employeeId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "NotFound" => NotFound("Medarbejderen blev ikke fundet."),
                "HasRelations" => Conflict("Medarbejderen kan ikke slettes, fordi den bruges i eksisterende registreringer."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return NoContent();
    }
}
