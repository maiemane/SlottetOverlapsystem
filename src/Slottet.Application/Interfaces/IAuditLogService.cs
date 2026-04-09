using Slottet.Application.DTOs.Audit;

namespace Slottet.Application.Interfaces;

public interface IAuditLogService
{
    Task<IReadOnlyList<AuditLogDto>> GetAuditLogsAsync(int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccessLogDto>> GetAccessLogsAsync(int take, CancellationToken cancellationToken = default);
}
