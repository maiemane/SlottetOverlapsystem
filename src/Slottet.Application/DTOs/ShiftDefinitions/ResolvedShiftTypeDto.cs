using Slottet.Domain.Enums;

namespace Slottet.Application.DTOs.ShiftDefinitions;

public sealed class ResolvedShiftTypeDto
{
    public int ShiftDefinitionId { get; set; }
    public ShiftType ShiftType { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public TimeOnly Time { get; set; }
}
