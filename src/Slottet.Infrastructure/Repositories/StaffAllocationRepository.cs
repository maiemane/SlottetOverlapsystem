using Microsoft.EntityFrameworkCore;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Infrastructure.Data;

namespace Slottet.Infrastructure.Repositories;

public sealed class StaffAllocationRepository : IStaffAllocationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public StaffAllocationRepository(ApplicationDbContext dbContext)
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

    public async Task<IReadOnlyList<Employee>> GetActiveEmployeesByIdsAsync(IReadOnlyCollection<int> employeeIds, CancellationToken cancellationToken = default)
    {
        if (employeeIds.Count == 0)
        {
            return [];
        }

        return await _dbContext.Employees
            .AsNoTracking()
            .Where(employee => employee.IsActive && employeeIds.Contains(employee.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> GetEmployeesByShiftAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        var employeeIds = await _dbContext.ShiftEmployees
            .AsNoTracking()
            .Where(shiftEmployee => shiftEmployee.ShiftId == shiftId)
            .Select(shiftEmployee => shiftEmployee.EmployeeId)
            .ToListAsync(cancellationToken);

        if (employeeIds.Count == 0)
        {
            return [];
        }

        return await _dbContext.Employees
            .AsNoTracking()
            .Where(employee => employeeIds.Contains(employee.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> GetEmployeesByCitizenAssignmentAsync(int shiftId, int citizenId, CancellationToken cancellationToken = default)
    {
        var employeeIds = await _dbContext.CitizenAssignments
            .AsNoTracking()
            .Where(assignment => assignment.ShiftId == shiftId && assignment.CitizenId == citizenId)
            .Select(assignment => assignment.EmployeeId)
            .ToListAsync(cancellationToken);

        if (employeeIds.Count == 0)
        {
            return [];
        }

        return await _dbContext.Employees
            .AsNoTracking()
            .Where(employee => employeeIds.Contains(employee.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetShiftEmployeeIdsAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ShiftEmployees
            .AsNoTracking()
            .Where(shiftEmployee => shiftEmployee.ShiftId == shiftId)
            .Select(shiftEmployee => shiftEmployee.EmployeeId)
            .ToListAsync(cancellationToken);
    }

    public async Task ReplaceShiftEmployeesAsync(int shiftId, IReadOnlyCollection<int> employeeIds, CancellationToken cancellationToken = default)
    {
        var existingAssignments = await _dbContext.ShiftEmployees
            .Where(shiftEmployee => shiftEmployee.ShiftId == shiftId)
            .ToListAsync(cancellationToken);

        _dbContext.ShiftEmployees.RemoveRange(existingAssignments);

        var newAssignments = employeeIds
            .Select(employeeId => new ShiftEmployee
            {
                ShiftId = shiftId,
                EmployeeId = employeeId
            });

        await _dbContext.ShiftEmployees.AddRangeAsync(newAssignments, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ReplaceCitizenAssignmentsAsync(int shiftId, int citizenId, IReadOnlyCollection<int> employeeIds, CancellationToken cancellationToken = default)
    {
        var existingAssignments = await _dbContext.CitizenAssignments
            .Where(assignment => assignment.ShiftId == shiftId && assignment.CitizenId == citizenId)
            .ToListAsync(cancellationToken);

        _dbContext.CitizenAssignments.RemoveRange(existingAssignments);

        var newAssignments = employeeIds
            .Select(employeeId => new CitizenAssignment
            {
                ShiftId = shiftId,
                CitizenId = citizenId,
                EmployeeId = employeeId
            });

        await _dbContext.CitizenAssignments.AddRangeAsync(newAssignments, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
