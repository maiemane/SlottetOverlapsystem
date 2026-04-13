using Microsoft.EntityFrameworkCore;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Infrastructure.Data;

namespace Slottet.Infrastructure.Repositories;

public sealed class ResponsibilityTypeRepository : IResponsibilityTypeRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ResponsibilityTypeRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ResponsibilityType>> GetResponsibilityTypesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ResponsibilityTypes
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<ResponsibilityType?> GetResponsibilityTypeByIdAsync(int responsibilityTypeId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ResponsibilityTypes
            .FirstOrDefaultAsync(type => type.Id == responsibilityTypeId, cancellationToken);
    }

    public Task<ResponsibilityType?> GetResponsibilityTypeByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return _dbContext.ResponsibilityTypes
            .FirstOrDefaultAsync(type => type.Name == name, cancellationToken);
    }

    public Task<bool> HasAssignmentsAsync(int responsibilityTypeId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ResponsibilityAssignments
            .AnyAsync(assignment => assignment.ResponsibilityTypeId == responsibilityTypeId, cancellationToken);
    }

    public async Task<ResponsibilityType> AddResponsibilityTypeAsync(ResponsibilityType responsibilityType, CancellationToken cancellationToken = default)
    {
        _dbContext.ResponsibilityTypes.Add(responsibilityType);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return responsibilityType;
    }

    public async Task<ResponsibilityType> UpdateResponsibilityTypeAsync(ResponsibilityType responsibilityType, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        return responsibilityType;
    }

    public async Task DeleteResponsibilityTypeAsync(ResponsibilityType responsibilityType, CancellationToken cancellationToken = default)
    {
        _dbContext.ResponsibilityTypes.Remove(responsibilityType);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
