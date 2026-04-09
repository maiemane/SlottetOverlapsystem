namespace Slottet.Application.DTOs.Citizens;

public sealed class CreateCitizenFixedMedicationResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public CreateCitizenFixedMedicationResponse? FixedMedication { get; set; }
}
