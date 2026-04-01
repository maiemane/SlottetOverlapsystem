using Microsoft.AspNetCore.Mvc;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;

namespace Slottet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CitizensController : ControllerBase
{
    private readonly ICitizenRepository _citizenRepository;

    public CitizensController(ICitizenRepository citizenRepository)
    {
        _citizenRepository = citizenRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Citizen>>> GetAll()
    {
        var citizens = await _citizenRepository.GetAllAsync();
        return Ok(citizens);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Citizen>> GetById(int id)
    {
        var citizen = await _citizenRepository.GetByIdAsync(id);

        if (citizen is null)
        {
            return NotFound();
        }

        return Ok(citizen);
    }
}