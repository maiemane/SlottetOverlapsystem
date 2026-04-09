using Slottet.Domain.Entities;

namespace Slottet.Application.Interfaces;

public interface IShiftDefinitionRepository
{
    Task<IReadOnlyList<ShiftDefinition>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ShiftDefinition?> GetByIdAsync(int shiftDefinitionId, CancellationToken cancellationToken = default);
    Task<ShiftDefinition> UpdateAsync(ShiftDefinition shiftDefinition, CancellationToken cancellationToken = default);
}
