using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slottet.Application.Interfaces;
using Slottet.Application.DTOs.History;

namespace Slottet.Api.Controllers;

[ApiController]
[Route("api/admin/history")]
[Authorize(Roles = "Admin")]
public class HistoryController : ControllerBase
{
    private readonly IHistoryService _historyService;

    public HistoryController(IHistoryService historyService)
    {
        _historyService = historyService;
    }

    /// <summary>
    /// Returns a flat list of history events across medication registrations
    /// and special events, filtered by date range, optional citizen and type.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetHistory(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? citizenId,
        [FromQuery] string? type,
        [FromQuery] int take = 100)
    {
        var fromUtc = (from ?? DateTime.UtcNow.AddDays(-30)).Date;
        var toUtc = (to ?? DateTime.UtcNow).Date.AddDays(1).AddTicks(-1); // inclusive end of day

        take = Math.Clamp(take, 1, 500);

        var events = await _historyService.GetHistoryAsync(
            fromUtc, toUtc, citizenId, type, take);

        return Ok(events);
    }
}