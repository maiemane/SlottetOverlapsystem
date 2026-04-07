namespace Slottet.Domain.Entities;

public class ResponsibilityAssignment
{
    public int Id { get; set; }
    public int EmployeeId { get; set; } //foreign key
    public int ShiftId { get; set; } //foreign key
    public int ResponsibilityTypeId { get; set; } //foreign key
}