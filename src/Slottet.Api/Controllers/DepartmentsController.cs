using Microsoft.AspNetCore.Mvc;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;

namespace Slottet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentRepository _departmentRepository;

    public DepartmentsController(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Department>>> GetAll()
    {
        var departments = await _departmentRepository.GetAllAsync();
        return Ok(departments);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Department>> GetById(int id)
    {
        var department = await _departmentRepository.GetByIdAsync(id);

        if (department is null)
        {
            return NotFound();
        }

        return Ok(department);
    }
}