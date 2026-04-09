namespace Slottet.Application.DTOs.Medications;

public sealed class CreateMedicationRegistrationResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public MedicationRegistrationDto? Registration { get; set; }
}
