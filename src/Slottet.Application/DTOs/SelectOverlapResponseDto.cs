using Slottet.Domain.Enums;

namespace Slottet.Application.DTOs;

public class SelectOverlapResponseDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public ShiftType ShiftType { get; set; }
    public string Message { get; set; } = string.Empty;
}