using Slottet.Domain.Enums;

namespace Slottet.Application.DTOs.Citizens;

public sealed class CreateCitizenRequest
{
    public string Name { get; set; } = string.Empty;
    public string ApartmentNumber { get; set; } = string.Empty;
    public TrafficLight TrafficLight { get; set; }
    public int DepartmentId { get; set; }
}
