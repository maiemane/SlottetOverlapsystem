using Slottet.Domain.Enums;

namespace Slottet.Application.DTOs;

public class SelectOverlapRequestDto
{
    public int DepartmentId { get; set; }
    public ShiftType ShiftType { get; set; }
}