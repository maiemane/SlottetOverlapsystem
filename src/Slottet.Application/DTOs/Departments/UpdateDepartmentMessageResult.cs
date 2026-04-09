namespace Slottet.Application.DTOs.Departments;

public sealed class UpdateDepartmentMessageResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public DepartmentDto? Department { get; set; }
}
