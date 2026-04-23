using Microsoft.EntityFrameworkCore;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Infrastructure.Data;

namespace Slottet.Infrastructure.Repositories;

public sealed class HistoryRepository : IHistoryRepository
{
    private readonly ApplicationDbContext _dbContext;

    public HistoryRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MedicinRegistration>> GetMedicinRegistrationsAsync(
        DateTime fromUtc,
        DateTime toUtc,
        int? citizenId,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.MedicinRegistrations
            .AsNoTracking()
            .Where(r => r.RegistrationTime >= fromUtc && r.RegistrationTime <= toUtc);

        if (citizenId.HasValue)
        {
            query = query.Where(r => r.CitizenId == citizenId.Value);
        }

        return await query
            .OrderByDescending(r => r.RegistrationTime)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SpecialEvent>> GetSpecialEventsAsync(
        DateTime fromUtc,
        DateTime toUtc,
        int? citizenId,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.SpecialEvents
            .AsNoTracking()
            .Where(e => e.EventTime >= fromUtc && e.EventTime <= toUtc);

        if (citizenId.HasValue)
        {
            query = query.Where(e => e.CitizenId == citizenId.Value);
        }

        return await query
            .OrderByDescending(e => e.EventTime)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Citizen>> GetCitizensByIdsAsync(
        IEnumerable<int> citizenIds,
        CancellationToken cancellationToken = default)
    {
        var ids = citizenIds.ToList();

        return await _dbContext.Citizens
            .AsNoTracking()
            .Where(c => ids.Contains(c.Id))
            .ToListAsync(cancellationToken);
    }
}
