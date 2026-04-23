namespace Slottet.Application.DTOs.History;

public sealed class HistoryEventDto
{
    public int Id { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public string CitizenName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}