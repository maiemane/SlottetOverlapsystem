using Microsoft.EntityFrameworkCore;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Infrastructure.Data;

namespace Slottet.Infrastructure.Repositories;

public sealed class PhoneAllocationRepository : IPhoneAllocationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PhoneAllocationRepository(ApplicationDbContext dbContext)
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

    public Task<Phone?> GetPhoneByIdAsync(int phoneId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Phones
            .AsNoTracking()
            .FirstOrDefaultAsync(phone => phone.Id == phoneId, cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetShiftEmployeeIdsAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ShiftEmployees
            .AsNoTracking()
            .Where(shiftEmployee => shiftEmployee.ShiftId == shiftId)
            .Select(shiftEmployee => shiftEmployee.EmployeeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PhoneAllocation>> GetPhoneAllocationsAsync(int shiftId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PhoneAllocations
            .AsNoTracking()
            .Where(allocation => allocation.ShiftId == shiftId)
            .ToListAsync(cancellationToken);
    }

    public Task<PhoneAllocation?> GetPhoneAllocationByIdAsync(int phoneAllocationId, CancellationToken cancellationToken = default)
    {
        return _dbContext.PhoneAllocations
            .FirstOrDefaultAsync(allocation => allocation.Id == phoneAllocationId, cancellationToken);
    }

    public Task<PhoneAllocation?> GetPhoneAllocationByShiftAndPhoneAsync(int shiftId, int phoneId, CancellationToken cancellationToken = default)
    {
        return _dbContext.PhoneAllocations
            .FirstOrDefaultAsync(allocation => allocation.ShiftId == shiftId && allocation.PhoneId == phoneId, cancellationToken);
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

    public async Task<IReadOnlyList<Phone>> GetPhonesByIdsAsync(IReadOnlyCollection<int> phoneIds, CancellationToken cancellationToken = default)
    {
        if (phoneIds.Count == 0)
        {
            return [];
        }

        return await _dbContext.Phones
            .AsNoTracking()
            .Where(phone => phoneIds.Contains(phone.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<PhoneAllocation> AddPhoneAllocationAsync(PhoneAllocation phoneAllocation, CancellationToken cancellationToken = default)
    {
        _dbContext.PhoneAllocations.Add(phoneAllocation);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return phoneAllocation;
    }

    public async Task<PhoneAllocation> UpdatePhoneAllocationAsync(PhoneAllocation phoneAllocation, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        return phoneAllocation;
    }

    public async Task DeletePhoneAllocationAsync(PhoneAllocation phoneAllocation, CancellationToken cancellationToken = default)
    {
        _dbContext.PhoneAllocations.Remove(phoneAllocation);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
