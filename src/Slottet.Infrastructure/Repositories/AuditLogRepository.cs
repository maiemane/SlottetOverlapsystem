using Microsoft.EntityFrameworkCore;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Infrastructure.Data;

namespace Slottet.Infrastructure.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AuditLogRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AuditLog>> GetAuditLogsAsync(int take, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AuditLogs
            .AsNoTracking()
            .OrderByDescending(log => log.OccurredAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AccessLog>> GetAccessLogsAsync(int take, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AccessLogs
            .AsNoTracking()
            .OrderByDescending(log => log.OccurredAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
