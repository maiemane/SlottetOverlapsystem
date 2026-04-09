namespace Slottet.Application.DTOs.Medications;

public sealed class DeleteMedicationRegistrationResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
}
