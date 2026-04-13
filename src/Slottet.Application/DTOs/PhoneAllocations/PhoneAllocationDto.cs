namespace Slottet.Application.DTOs.PhoneAllocations;

public sealed class PhoneAllocationDto
{
    public int Id { get; set; }
    public int ShiftId { get; set; }
    public int PhoneId { get; set; }
    public string PhoneNameOrNumber { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
}
