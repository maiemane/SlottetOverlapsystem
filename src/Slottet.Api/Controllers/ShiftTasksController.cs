using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slottet.Application.DTOs.ShiftTasks;
using Slottet.Application.Interfaces;

namespace Slottet.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Medarbejder,Vagtansvarlig")]
[Route("api/shifts/{shiftId:int}/tasks")]
public sealed class ShiftTasksController : ControllerBase
{
    private readonly IShiftTaskService _shiftTaskService;

    public ShiftTasksController(IShiftTaskService shiftTaskService)
    {
        _shiftTaskService = shiftTaskService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ShiftTaskDto>>> Get(
        int shiftId,
        CancellationToken cancellationToken)
    {
        var tasks = await _shiftTaskService.GetByShiftAsync(shiftId, cancellationToken);

        if (tasks is null)
        {
            return NotFound("Vagten blev ikke fundet.");
        }

        return Ok(tasks);
    }

    [HttpGet("{shiftTaskId:int}")]
    public async Task<ActionResult<ShiftTaskDto>> GetById(
        int shiftId,
        int shiftTaskId,
        CancellationToken cancellationToken)
    {
        var task = await _shiftTaskService.GetByIdAsync(shiftId, shiftTaskId, cancellationToken);

        if (task is null)
        {
            return NotFound("Opgaven blev ikke fundet.");
        }

        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<ShiftTaskDto>> Create(
        int shiftId,
        [FromBody] CreateShiftTaskRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _shiftTaskService.CreateAsync(shiftId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "InvalidRequest" => BadRequest("Beskrivelse er paakraevet."),
                "ShiftNotFound" => NotFound("Vagten blev ikke fundet."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return CreatedAtAction(nameof(GetById), new { shiftId, shiftTaskId = result.ShiftTask!.Id }, result.ShiftTask);
    }

    [HttpPut("{shiftTaskId:int}")]
    public async Task<ActionResult<ShiftTaskDto>> Update(
        int shiftId,
        int shiftTaskId,
        [FromBody] UpdateShiftTaskRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _shiftTaskService.UpdateAsync(shiftId, shiftTaskId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "InvalidRequest" => BadRequest("Beskrivelse og gyldig status er paakraevet."),
                "ShiftNotFound" => NotFound("Vagten blev ikke fundet."),
                "ShiftTaskNotFound" => NotFound("Opgaven blev ikke fundet."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return Ok(result.ShiftTask);
    }

    [HttpDelete("{shiftTaskId:int}")]
    public async Task<IActionResult> Delete(
        int shiftId,
        int shiftTaskId,
        CancellationToken cancellationToken)
    {
        var result = await _shiftTaskService.DeleteAsync(shiftId, shiftTaskId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "ShiftNotFound" => NotFound("Vagten blev ikke fundet."),
                "ShiftTaskNotFound" => NotFound("Opgaven blev ikke fundet."),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        return NoContent();
    }
}
