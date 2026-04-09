using Microsoft.EntityFrameworkCore;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Infrastructure.Data;

namespace Slottet.Infrastructure.Repositories;

public sealed class DepartmentRepository : IDepartmentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public DepartmentRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Departments
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<Department?> GetByIdAsync(int departmentId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Departments
            .FirstOrDefaultAsync(department => department.Id == departmentId, cancellationToken);
    }

    public async Task<Department> UpdateAsync(Department department, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        return department;
    }
}
