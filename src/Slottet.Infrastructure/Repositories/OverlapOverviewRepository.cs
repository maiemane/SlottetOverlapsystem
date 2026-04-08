using Microsoft.EntityFrameworkCore;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Infrastructure.Data;

namespace Slottet.Infrastructure.Repositories;

public sealed class OverlapOverviewRepository : IOverlapOverviewRepository
{
    private readonly ApplicationDbContext _dbContext;

    public OverlapOverviewRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Department?> GetDepartmentByIdAsync(int departmentId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(department => department.Id == departmentId, cancellationToken);
    }

    public Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Shifts
            .AsNoTracking()
            .FirstOrDefaultAsync(shift => shift.Id == shiftId, cancellationToken);
    }

    public async Task<IReadOnlyList<Citizen>> GetActiveCitizensByDepartmentAsync(int departmentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Citizens
            .AsNoTracking()
            .Where(citizen => citizen.DepartmentId == departmentId && citizen.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MedicinRegistration>> GetMedicationsByShiftAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.MedicinRegistrations
            .AsNoTracking()
            .Where(medication => medication.ShiftId == shiftId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SpecialEvent>> GetSpecialEventsByShiftAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SpecialEvents
            .AsNoTracking()
            .Where(specialEvent => specialEvent.ShiftId == shiftId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CitizenAssignment>> GetCitizenAssignmentsByShiftAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CitizenAssignments
            .AsNoTracking()
            .Where(assignment => assignment.ShiftId == shiftId)
            .ToListAsync(cancellationToken);
    }
}
