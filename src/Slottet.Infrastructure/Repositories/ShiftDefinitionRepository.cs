using Microsoft.EntityFrameworkCore;
using Slottet.Application.Interfaces;
using Slottet.Domain.Entities;
using Slottet.Infrastructure.Data;

namespace Slottet.Infrastructure.Repositories;

public sealed class ShiftDefinitionRepository : IShiftDefinitionRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ShiftDefinitionRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ShiftDefinition>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.ShiftDefinitions
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<ShiftDefinition?> GetByIdAsync(int shiftDefinitionId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ShiftDefinitions
            .FirstOrDefaultAsync(definition => definition.Id == shiftDefinitionId, cancellationToken);
    }

    public async Task<ShiftDefinition> UpdateAsync(ShiftDefinition shiftDefinition, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        return shiftDefinition;
    }
}
