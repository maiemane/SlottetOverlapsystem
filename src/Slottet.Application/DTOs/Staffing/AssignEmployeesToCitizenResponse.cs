namespace Slottet.Application.DTOs.Staffing;

public sealed class AssignEmployeesToCitizenResponse
{
    public int ShiftId { get; set; }
    public int CitizenId { get; set; }
    public List<int> EmployeeIds { get; set; } = [];
}
