using Slottet.Domain.Enums;

namespace Slottet.Application.DTOs.Staffing;

public sealed class CitizenAssignmentBoardShiftDto
{
    public int ShiftId { get; set; }
    public ShiftType ShiftType { get; set; }
    public List<int> AssignedEmployeeIds { get; set; } = [];
}
