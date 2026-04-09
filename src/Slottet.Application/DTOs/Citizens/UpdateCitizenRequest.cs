using Slottet.Domain.Enums;

namespace Slottet.Application.DTOs.Citizens;

public sealed class UpdateCitizenRequest
{
    public string Name { get; set; } = string.Empty;
    public string ApartmentNumber { get; set; } = string.Empty;
    public TrafficLight TrafficLight { get; set; }
    public int DepartmentId { get; set; }
    public bool IsActive { get; set; } = true;
}
