namespace Slottet.Domain.Entities;
using Slottet.Domain.Enums;

public class ShiftTask
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public int ShiftId { get; set; } // foreign key
    public required ShiftTaskStatus Status {get; set;}
}