using Microsoft.EntityFrameworkCore;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Infrastructure.Data;

namespace Slottet.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _dbContext;

    public EmployeeRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Employees
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Employee?> GetActiveByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToUpper();

        return await _dbContext.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email.ToUpper() == normalizedEmail && x.IsActive, cancellationToken);
    }

    public Task<Employee?> GetByIdAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Employees
            .FirstOrDefaultAsync(x => x.Id == employeeId, cancellationToken);
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToUpper();

        return _dbContext.Employees
            .AsNoTracking()
            .AnyAsync(x => x.Email.ToUpper() == normalizedEmail, cancellationToken);
    }

    public async Task<Employee> CreateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return employee;
    }

    public async Task<bool> DeleteAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        _dbContext.Employees.Remove(employee);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            _dbContext.Entry(employee).State = EntityState.Unchanged;
            return false;
        }
    }
}
