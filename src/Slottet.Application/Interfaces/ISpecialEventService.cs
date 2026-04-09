using Slottet.Application.DTOs.SpecialEvents;

namespace Slottet.Application.Interfaces;

public interface ISpecialEventService
{
    Task<IReadOnlyList<SpecialEventDto>?> GetByCitizenAndShiftAsync(int shiftId, int citizenId, CancellationToken cancellationToken = default);
    Task<SpecialEventDto?> GetByIdAsync(int shiftId, int citizenId, int specialEventId, CancellationToken cancellationToken = default);
    Task<CreateSpecialEventResult> CreateAsync(int shiftId, int citizenId, CreateSpecialEventRequest request, CancellationToken cancellationToken = default);
    Task<UpdateSpecialEventResult> UpdateAsync(int shiftId, int citizenId, int specialEventId, UpdateSpecialEventRequest request, CancellationToken cancellationToken = default);
    Task<DeleteSpecialEventResult> DeleteAsync(int shiftId, int citizenId, int specialEventId, CancellationToken cancellationToken = default);
}
