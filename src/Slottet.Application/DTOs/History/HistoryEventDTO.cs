namespace Slottet.Application.DTOs.History;

/// <summary>
/// Flattened history event returned to the frontend.
/// EventType is either "Medication" or "SpecialEvent".
/// </summary>
public sealed class HistoryEventDto
{
    public int Id { get; init; }
    public DateTime OccurredAtUtc { get; init; }
    public string CitizenName { get; init; } = string.Empty;

    /// <summary>"Medication" or "SpecialEvent"</summary>
    public string EventType { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;
    public int? EmployeeId { get; init; }
}