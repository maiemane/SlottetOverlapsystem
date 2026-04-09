using Slottet.Domain.Enums;

namespace Slottet.Application.DTOs.ShiftDefinitions;

public sealed class UpdateShiftDefinitionRequestDto
{
    public ShiftType ShiftType { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsActive { get; set; } = true;
}
