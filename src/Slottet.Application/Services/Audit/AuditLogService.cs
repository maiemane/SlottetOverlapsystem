using Slottet.Application.DTOs.Audit;
using Slottet.Application.Interfaces;

namespace Slottet.Application.Services.Audit;

public sealed class AuditLogService : IAuditLogService
{
    private const int DefaultTake = 100;
    private const int MaxTake = 500;

    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetAuditLogsAsync(int take, CancellationToken cancellationToken = default)
    {
        var resolvedTake = ResolveTake(take);
        var auditLogs = await _auditLogRepository.GetAuditLogsAsync(resolvedTake, cancellationToken);

        return auditLogs
            .OrderByDescending(log => log.OccurredAtUtc)
            .Select(log => new AuditLogDto
            {
                Id = log.Id,
                OccurredAtUtc = log.OccurredAtUtc,
                EmployeeId = log.EmployeeId,
                Action = log.Action,
                EntityName = log.EntityName,
                EntityId = log.EntityId,
                OldValuesJson = log.OldValuesJson,
                NewValuesJson = log.NewValuesJson,
                RequestPath = log.RequestPath,
                CorrelationId = log.CorrelationId
            })
            .ToList();
    }

    public async Task<IReadOnlyList<AccessLogDto>> GetAccessLogsAsync(int take, CancellationToken cancellationToken = default)
    {
        var resolvedTake = ResolveTake(take);
        var accessLogs = await _auditLogRepository.GetAccessLogsAsync(resolvedTake, cancellationToken);

        return accessLogs
            .OrderByDescending(log => log.OccurredAtUtc)
            .Select(log => new AccessLogDto
            {
                Id = log.Id,
                OccurredAtUtc = log.OccurredAtUtc,
                EmployeeId = log.EmployeeId,
                HttpMethod = log.HttpMethod,
                RequestPath = log.RequestPath,
                QueryString = log.QueryString,
                StatusCode = log.StatusCode,
                CorrelationId = log.CorrelationId
            })
            .ToList();
    }

    private static int ResolveTake(int take)
    {
        if (take <= 0)
        {
            return DefaultTake;
        }

        return Math.Min(take, MaxTake);
    }
}
