namespace Slottet.Application.DTOs.Phones;

public sealed class CreatePhoneResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public PhoneDto? Phone { get; set; }
}
