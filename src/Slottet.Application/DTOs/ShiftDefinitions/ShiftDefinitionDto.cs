using Slottet.Domain.Enums;

namespace Slottet.Application.DTOs.ShiftDefinitions;

public sealed class ShiftDefinitionDto
{
    public int Id { get; set; }
    public ShiftType ShiftType { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsActive { get; set; }
}
