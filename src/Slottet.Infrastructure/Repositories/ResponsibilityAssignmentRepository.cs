using Microsoft.EntityFrameworkCore;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Infrastructure.Data;

namespace Slottet.Infrastructure.Repositories;

public sealed class ResponsibilityAssignmentRepository : IResponsibilityAssignmentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ResponsibilityAssignmentRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Shifts
            .AsNoTracking()
            .FirstOrDefaultAsync(shift => shift.Id == shiftId, cancellationToken);
    }

    public Task<Employee?> GetEmployeeByIdAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(employee => employee.Id == employeeId, cancellationToken);
    }

    public Task<ResponsibilityType?> GetResponsibilityTypeByIdAsync(int responsibilityTypeId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ResponsibilityTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(type => type.Id == responsibilityTypeId, cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetShiftEmployeeIdsAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ShiftEmployees
            .AsNoTracking()
            .Where(shiftEmployee => shiftEmployee.ShiftId == shiftId)
            .Select(shiftEmployee => shiftEmployee.EmployeeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ResponsibilityAssignment>> GetResponsibilityAssignmentsAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ResponsibilityAssignments
            .AsNoTracking()
            .Where(assignment => assignment.ShiftId == shiftId)
            .ToListAsync(cancellationToken);
    }

    public Task<ResponsibilityAssignment?> GetResponsibilityAssignmentByIdAsync(int responsibilityAssignmentId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ResponsibilityAssignments
            .FirstOrDefaultAsync(assignment => assignment.Id == responsibilityAssignmentId, cancellationToken);
    }

    public Task<ResponsibilityAssignment?> GetResponsibilityAssignmentByShiftAndTypeAsync(int shiftId, int responsibilityTypeId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ResponsibilityAssignments
            .FirstOrDefaultAsync(assignment => assignment.ShiftId == shiftId && assignment.ResponsibilityTypeId == responsibilityTypeId, cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> GetEmployeesByIdsAsync(IReadOnlyCollection<int> employeeIds, CancellationToken cancellationToken = default)
    {
        if (employeeIds.Count == 0)
        {
            return [];
        }

        return await _dbContext.Employees
            .AsNoTracking()
            .Where(employee => employeeIds.Contains(employee.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ResponsibilityType>> GetResponsibilityTypesByIdsAsync(IReadOnlyCollection<int> responsibilityTypeIds, CancellationToken cancellationToken = default)
    {
        if (responsibilityTypeIds.Count == 0)
        {
            return [];
        }

        return await _dbContext.ResponsibilityTypes
            .AsNoTracking()
            .Where(type => responsibilityTypeIds.Contains(type.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<ResponsibilityAssignment> AddResponsibilityAssignmentAsync(ResponsibilityAssignment responsibilityAssignment, CancellationToken cancellationToken = default)
    {
        _dbContext.ResponsibilityAssignments.Add(responsibilityAssignment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return responsibilityAssignment;
    }

    public async Task<ResponsibilityAssignment> UpdateResponsibilityAssignmentAsync(ResponsibilityAssignment responsibilityAssignment, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        return responsibilityAssignment;
    }

    public async Task DeleteResponsibilityAssignmentAsync(ResponsibilityAssignment responsibilityAssignment, CancellationToken cancellationToken = default)
    {
        _dbContext.ResponsibilityAssignments.Remove(responsibilityAssignment);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
