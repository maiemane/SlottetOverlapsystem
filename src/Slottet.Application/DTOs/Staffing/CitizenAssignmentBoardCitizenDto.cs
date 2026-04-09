namespace Slottet.Application.DTOs.Staffing;

public sealed class CitizenAssignmentBoardCitizenDto
{
    public int CitizenId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApartmentNumber { get; set; } = string.Empty;
    public List<CitizenAssignmentBoardShiftDto> Shifts { get; set; } = [];
}
