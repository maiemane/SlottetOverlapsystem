namespace Slottet.Application.DTOs.Employees;

public sealed class DeleteEmployeeResultDto
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
}
