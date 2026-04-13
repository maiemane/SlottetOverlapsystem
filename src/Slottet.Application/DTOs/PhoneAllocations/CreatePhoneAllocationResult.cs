namespace Slottet.Application.DTOs.PhoneAllocations;

public sealed class CreatePhoneAllocationResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public PhoneAllocationDto? PhoneAllocation { get; set; }
}
