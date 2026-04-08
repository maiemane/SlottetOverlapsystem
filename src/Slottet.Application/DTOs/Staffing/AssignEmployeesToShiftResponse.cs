namespace Slottet.Application.DTOs.Staffing;

public sealed class AssignEmployeesToShiftResponse
{
    public int ShiftId { get; set; }
    public List<int> EmployeeIds { get; set; } = [];
}
