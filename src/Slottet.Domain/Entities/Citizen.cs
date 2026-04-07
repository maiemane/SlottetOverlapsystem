using Slottet.Domain.Enums;
namespace Slottet.Domain.Entities;

public class Citizen
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public TrafficLight TrafficLight { get; set; }
    public int DepartmentId { get; set; } //foreign key
    public bool IsActive { get; set; } = true;
}