using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Slottet.Application.DTOs.Audit;
using Slottet.Application.Interfaces;

namespace Slottet.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/logs")]
public sealed class AdminLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AdminLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet("audit")]
    public async Task<ActionResult<IReadOnlyList<AuditLogDto>>> GetAuditLogs(
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var logs = await _auditLogService.GetAuditLogsAsync(take, cancellationToken);
        return Ok(logs);
    }

    [HttpGet("access")]
    public async Task<ActionResult<IReadOnlyList<AccessLogDto>>> GetAccessLogs(
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var logs = await _auditLogService.GetAccessLogsAsync(take, cancellationToken);
        return Ok(logs);
    }
}
