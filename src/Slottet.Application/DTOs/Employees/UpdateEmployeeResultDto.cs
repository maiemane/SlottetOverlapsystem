namespace Slottet.Application.DTOs.Employees;

public sealed class UpdateEmployeeResultDto
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public EmployeeDto? Employee { get; set; }
}
