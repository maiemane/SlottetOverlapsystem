namespace Slottet.Domain.Entities;

public class CitizenAssignment
{
    public int Id { get; set; }
    public int EmployeeId { get; set; } //foreign key
    public int CitizenId { get; set; } //foreign key
    public int ShiftId { get; set; } //foreign key
}