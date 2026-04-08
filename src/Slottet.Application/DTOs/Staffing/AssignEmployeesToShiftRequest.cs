namespace Slottet.Application.DTOs.Staffing;

public sealed class AssignEmployeesToShiftRequest
{
    public List<int> EmployeeIds { get; set; } = [];
}
