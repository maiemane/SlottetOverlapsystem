using Microsoft.EntityFrameworkCore;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Domain.Enums;
using Slottet.Infrastructure.Data;

namespace Slottet.Infrastructure.Repositories;

public sealed class StaffAllocationRepository : IStaffAllocationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public StaffAllocationRepository(ApplicationDbContext dbContext)
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

    public Task<Citizen?> GetCitizenByIdAsync(int citizenId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Citizens
            .AsNoTracking()
            .FirstOrDefaultAsync(citizen => citizen.Id == citizenId, cancellationToken);
    }

    public async Task<IReadOnlyList<Citizen>> GetActiveCitizensByDepartmentAsync(int departmentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Citizens
            .AsNoTracking()
            .Where(citizen => citizen.IsActive && citizen.DepartmentId == departmentId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> GetActiveEmployeesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Employees
            .AsNoTracking()
            .Where(employee => employee.IsActive)
            .ToListAsync(cancellationToken);
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

    public async Task<IReadOnlyList<Shift>> GetShiftsByDepartmentAndDateAsync(int departmentId, DateTime date, CancellationToken cancellationToken = default)
    {
        var targetDate = date.Date;
        var nextDate = targetDate.AddDays(1);

        return await _dbContext.Shifts
            .Where(shift => shift.DepartmentId == departmentId && shift.Date >= targetDate && shift.Date < nextDate)
            .OrderBy(shift => shift.Type)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Shift>> EnsureShiftsForDepartmentAndDateAsync(int departmentId, DateTime date, CancellationToken cancellationToken = default)
    {
        var targetDate = date.Date;
        var nextDate = targetDate.AddDays(1);
        var shifts = await _dbContext.Shifts
            .Where(shift => shift.DepartmentId == departmentId && shift.Date >= targetDate && shift.Date < nextDate)
            .ToListAsync(cancellationToken);

        var missingTypes = Enum.GetValues<ShiftType>()
            .Except(shifts.Select(shift => shift.Type))
            .ToList();

        if (missingTypes.Count > 0)
        {
            var newShifts = missingTypes
                .Select(shiftType => new Shift
                {
                    DepartmentId = departmentId,
                    Date = targetDate,
                    Type = shiftType
                })
                .ToList();

            await _dbContext.Shifts.AddRangeAsync(newShifts, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            shifts.AddRange(newShifts);
        }

        return shifts
            .OrderBy(shift => shift.Type)
            .ToList();
    }

    public async Task<IReadOnlyList<CitizenAssignment>> GetCitizenAssignmentsByShiftIdsAsync(IReadOnlyCollection<int> shiftIds, CancellationToken cancellationToken = default)
    {
        if (shiftIds.Count == 0)
        {
            return [];
        }

        return await _dbContext.CitizenAssignments
            .AsNoTracking()
            .Where(assignment => shiftIds.Contains(assignment.ShiftId))
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
