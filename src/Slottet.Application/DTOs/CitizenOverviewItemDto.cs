using System.Collections.Generic;

namespace Slottet.Application.DTOs;

public class CitizenOverviewItemDto
{
    public int CitizenId { get; set; }
    public string CitizenName { get; set; } = string.Empty;
    public string TrafficLightStatus { get; set; } = string.Empty;
    public string MedicationStatus { get; set; } = string.Empty;
    public List<string> AssignedEmployees { get; set; } = new();
    public string SpecialIncidentSummary { get; set; } = string.Empty;
}