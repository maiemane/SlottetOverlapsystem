namespace Slottet.Application.DTOs.Phones;

public sealed class UpdatePhoneRequest
{
    public string NameOrNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
