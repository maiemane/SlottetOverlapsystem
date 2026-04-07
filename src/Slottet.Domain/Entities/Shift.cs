using Slottet.Domain.Enums;

namespace Slottet.Domain.Entities;

public class Shift
{
    public int Id { get; set; }
    //TODO: måske tilføje denne
    // public Employee? Employee { get; set; } //navigation property
    public DateTime Date { get; set; }
    public ShiftType Type { get; set; }
    public int DepartmentId { get; set; } //foreign key
    //TODO: Måske tilføje denne
    // public Department Department { get; set; }
}