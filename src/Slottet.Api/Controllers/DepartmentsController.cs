using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slottet.Application.DTOs.Departments;
using Slottet.Application.Interfaces;

namespace Slottet.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/departments")]
public sealed class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DepartmentDto>>> GetAll(CancellationToken cancellationToken)
    {
        var departments = await _departmentService.GetAllAsync(cancellationToken);
        return Ok(departments);
    }

    [HttpGet("{departmentId:int}")]
    public async Task<ActionResult<DepartmentDto>> GetById(int departmentId, CancellationToken cancellationToken)
    {
        var department = await _departmentService.GetByIdAsync(departmentId, cancellationToken);

        if (department is null)
        {
            return NotFound("Afdelingen blev ikke fundet.");
        }

        return Ok(department);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{departmentId:int}/message")]
    public async Task<ActionResult<DepartmentDto>> UpdateMessage(
        int departmentId,
        [FromBody] UpdateDepartmentMessageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _departmentService.UpdateMessageAsync(departmentId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "NotFound" => NotFound("Afdelingen blev ikke fundet."),
                "MessageTooLong" => BadRequest("Beskeden er for lang."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return Ok(result.Department);
    }
}
