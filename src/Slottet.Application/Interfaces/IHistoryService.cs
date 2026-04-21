using Slottet.Application.DTOs.History;

namespace Slottet.Application.Interfaces;

/// <summary>
/// Aggregates history events from medication registrations and special events
/// for admin reporting and CSV export.
/// </summary>
public interface IHistoryService
{
    Task<IReadOnlyList<HistoryEventDto>> GetHistoryAsync(
        DateTime fromUtc,
        DateTime toUtc,
        int? citizenId,
        string? eventType,
        int take);
}