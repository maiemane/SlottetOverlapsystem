using Slottet.Domain.Enums;

namespace Slottet.Application.DTOs.Overlap;

public sealed class CitizenOverlapItemDto
{
    public int CitizenId { get; set; }
    public string CitizenName { get; set; } = string.Empty;
    public TrafficLight TrafficLight { get; set; }
    public string ApartmentNumber { get; set; } = string.Empty;
    public List<CitizenMedicationDto> Medications { get; set; } = [];
    public List<CitizenSpecialEventDto> SpecialEvents { get; set; } = [];
    public List<AssignedEmployeeDto> AssignedEmployees { get; set; } = [];
}
