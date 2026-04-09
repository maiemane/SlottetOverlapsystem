using Slottet.Domain.Entities;

namespace Slottet.Application.Interfaces;

public interface IAuditLogRepository
{
    Task<IReadOnlyList<AuditLog>> GetAuditLogsAsync(int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccessLog>> GetAccessLogsAsync(int take, CancellationToken cancellationToken = default);
}
