namespace Slottet.Application.DTOs.Citizens;

public sealed class UpdateCitizenFixedMedicationResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public CitizenFixedMedicationDto? FixedMedication { get; set; }
}
