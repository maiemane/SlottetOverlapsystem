namespace Slottet.Application.DTOs.PhoneAllocations;

public sealed class CreatePhoneAllocationRequest
{
    public int PhoneId { get; set; }
    public int EmployeeId { get; set; }
}
