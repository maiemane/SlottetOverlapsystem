using Slottet.Domain.Entities;

namespace Slottet.Application.Interfaces;

public interface ISpecialEventRepository
{
    Task<Shift?> GetShiftByIdAsync(int shiftId, CancellationToken cancellationToken = default);
    Task<Citizen?> GetCitizenByIdAsync(int citizenId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SpecialEvent>> GetSpecialEventsAsync(int shiftId, int citizenId, CancellationToken cancellationToken = default);
    Task<SpecialEvent?> GetSpecialEventByIdAsync(int specialEventId, CancellationToken cancellationToken = default);
    Task<SpecialEvent> AddSpecialEventAsync(SpecialEvent specialEvent, CancellationToken cancellationToken = default);
    Task<SpecialEvent> UpdateSpecialEventAsync(SpecialEvent specialEvent, CancellationToken cancellationToken = default);
    Task DeleteSpecialEventAsync(SpecialEvent specialEvent, CancellationToken cancellationToken = default);
}
