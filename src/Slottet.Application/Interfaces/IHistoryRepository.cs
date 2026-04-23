using Slottet.Domain.Entities;

namespace Slottet.Application.Interfaces;

public interface IHistoryRepository
{
    Task<IReadOnlyList<MedicinRegistration>> GetMedicinRegistrationsAsync(DateTime fromUtc, DateTime toUtc, int? citizenId, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SpecialEvent>> GetSpecialEventsAsync(DateTime fromUtc, DateTime toUtc, int? citizenId, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Citizen>> GetCitizensByIdsAsync(IEnumerable<int> citizenIds, CancellationToken cancellationToken = default);
}