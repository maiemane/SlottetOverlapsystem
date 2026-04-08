namespace Slottet.Application.DTOs.Staffing;

public sealed class AssignEmployeesToShiftResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public AssignEmployeesToShiftResponse? Assignment { get; set; }
}
