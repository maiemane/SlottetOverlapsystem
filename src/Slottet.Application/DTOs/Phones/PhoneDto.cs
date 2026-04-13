namespace Slottet.Application.DTOs.Phones;

public sealed class PhoneDto
{
    public int Id { get; set; }
    public string NameOrNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
