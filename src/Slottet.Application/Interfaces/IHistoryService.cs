using Slottet.Application.DTOs.History;

namespace Slottet.Application.Interfaces;

public interface IHistoryService
{
    Task<IReadOnlyList<HistoryEventDto>> GetHistoryAsync(DateTime fromUtc, DateTime toUtc, int? citizenId, string? eventType, int take, CancellationToken cancellationToken = default);
}