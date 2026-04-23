using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slottet.Application.DTOs.History;
using Slottet.Application.Interfaces;

namespace Slottet.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/history")]
public sealed class HistoryController : ControllerBase
{
    private readonly IHistoryService _historyService;

    public HistoryController(IHistoryService historyService)
    {
        _historyService = historyService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HistoryEventDto>>> GetHistory(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? citizenId,
        [FromQuery] string? type,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var fromUtc = (from ?? DateTime.UtcNow.AddDays(-30)).Date;
        var toUtc = (to ?? DateTime.UtcNow).Date.AddDays(1).AddTicks(-1);

        var events = await _historyService.GetHistoryAsync(
            fromUtc, toUtc, citizenId, type, take, cancellationToken);

        return Ok(events);
    }
}