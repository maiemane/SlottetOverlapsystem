namespace Slottet.Domain.Entities;

public class Phone
{
    public int Id { get; set; }
    public string NameOrNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }  = true;
    
}