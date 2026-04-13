namespace Slottet.Application.DTOs.PhoneAllocations;

public sealed class UpdatePhoneAllocationRequest
{
    public int PhoneId { get; set; }
    public int EmployeeId { get; set; }
}
