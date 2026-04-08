using Microsoft.EntityFrameworkCore;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Infrastructure.Data;

namespace Slottet.Infrastructure.Repositories;

public sealed class CitizenCreationRepository : ICitizenCreationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CitizenCreationRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Department?> GetDepartmentByIdAsync(int departmentId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(department => department.Id == departmentId, cancellationToken);
    }

    public async Task<Citizen> AddCitizenAsync(Citizen citizen, CancellationToken cancellationToken = default)
    {
        _dbContext.Citizens.Add(citizen);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return citizen;
    }
}
