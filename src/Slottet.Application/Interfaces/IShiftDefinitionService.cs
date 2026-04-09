using Slottet.Application.DTOs.ShiftDefinitions;
using Slottet.Domain.Enums;

namespace Slottet.Application.Interfaces;

public interface IShiftDefinitionService
{
    Task<IReadOnlyList<ShiftDefinitionDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ShiftDefinitionDto?> GetByIdAsync(int shiftDefinitionId, CancellationToken cancellationToken = default);
    Task<UpdateShiftDefinitionResultDto> UpdateAsync(int shiftDefinitionId, UpdateShiftDefinitionRequestDto request, CancellationToken cancellationToken = default);
    Task<ResolvedShiftTypeDto?> ResolveByTimeAsync(TimeOnly time, CancellationToken cancellationToken = default);
    Task<ShiftType?> ResolveShiftTypeAsync(TimeOnly time, CancellationToken cancellationToken = default);
}
