namespace Slottet.Application.DTOs.Employees;

public sealed class CreateEmployeeResultDto
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public EmployeeDto? Employee { get; set; }
}
