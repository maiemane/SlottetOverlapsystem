namespace Slottet.Domain.Entities;

public class PhoneAllocation
{
    public int Id { get; set; }
    public int EmployeeId { get; set; } //foreign key
    public int ShiftId  { get; set; } //foreign key
    public int PhoneId { get; set; } //foreign key
}