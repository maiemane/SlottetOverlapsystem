namespace Slottet.Realtime;

public sealed record OverviewChangedMessage(
    Guid SourceId,
    int? DepartmentId,
    int? ShiftId,
    DateTime SelectedDate,
    string ChangeType);
