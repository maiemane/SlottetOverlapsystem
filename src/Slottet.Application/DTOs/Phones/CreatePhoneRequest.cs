namespace Slottet.Application.DTOs.Phones;

public sealed class CreatePhoneRequest
{
    public string NameOrNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
