using Microsoft.EntityFrameworkCore;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Infrastructure.Data;

namespace Slottet.Infrastructure.Repositories;

public sealed class ShiftTaskRepository : IShiftTaskRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ShiftTaskRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Shifts
            .AsNoTracking()
            .FirstOrDefaultAsync(shift => shift.Id == shiftId, cancellationToken);
    }

    public async Task<IReadOnlyList<ShiftTask>> GetShiftTasksAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ShiftTasks
            .AsNoTracking()
            .Where(task => task.ShiftId == shiftId)
            .ToListAsync(cancellationToken);
    }

    public Task<ShiftTask?> GetShiftTaskByIdAsync(int shiftTaskId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ShiftTasks
            .FirstOrDefaultAsync(task => task.Id == shiftTaskId, cancellationToken);
    }

    public async Task<ShiftTask> AddShiftTaskAsync(ShiftTask shiftTask, CancellationToken cancellationToken = default)
    {
        _dbContext.ShiftTasks.Add(shiftTask);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return shiftTask;
    }

    public async Task<ShiftTask> UpdateShiftTaskAsync(ShiftTask shiftTask, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        return shiftTask;
    }

    public async Task DeleteShiftTaskAsync(ShiftTask shiftTask, CancellationToken cancellationToken = default)
    {
        _dbContext.ShiftTasks.Remove(shiftTask);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
