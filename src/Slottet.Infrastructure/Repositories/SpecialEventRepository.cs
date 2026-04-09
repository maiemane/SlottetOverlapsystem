using Microsoft.EntityFrameworkCore;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Infrastructure.Data;

namespace Slottet.Infrastructure.Repositories;

public sealed class SpecialEventRepository : ISpecialEventRepository
{
    private readonly ApplicationDbContext _dbContext;

    public SpecialEventRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Shifts
            .AsNoTracking()
            .FirstOrDefaultAsync(shift => shift.Id == shiftId, cancellationToken);
    }

    public Task<Citizen?> GetCitizenByIdAsync(int citizenId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Citizens
            .AsNoTracking()
            .FirstOrDefaultAsync(citizen => citizen.Id == citizenId, cancellationToken);
    }

    public async Task<IReadOnlyList<SpecialEvent>> GetSpecialEventsAsync(int shiftId, int citizenId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SpecialEvents
            .AsNoTracking()
            .Where(specialEvent => specialEvent.ShiftId == shiftId && specialEvent.CitizenId == citizenId)
            .ToListAsync(cancellationToken);
    }

    public Task<SpecialEvent?> GetSpecialEventByIdAsync(int specialEventId, CancellationToken cancellationToken = default)
    {
        return _dbContext.SpecialEvents
            .FirstOrDefaultAsync(specialEvent => specialEvent.Id == specialEventId, cancellationToken);
    }

    public async Task<SpecialEvent> AddSpecialEventAsync(SpecialEvent specialEvent, CancellationToken cancellationToken = default)
    {
        _dbContext.SpecialEvents.Add(specialEvent);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return specialEvent;
    }

    public async Task<SpecialEvent> UpdateSpecialEventAsync(SpecialEvent specialEvent, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        return specialEvent;
    }

    public async Task DeleteSpecialEventAsync(SpecialEvent specialEvent, CancellationToken cancellationToken = default)
    {
        _dbContext.SpecialEvents.Remove(specialEvent);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
