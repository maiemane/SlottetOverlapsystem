namespace Slottet.Application.DTOs.Phones;

public sealed class UpdatePhoneResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public PhoneDto? Phone { get; set; }
}
