namespace Slottet.Application.DTOs.Citizens;

public sealed class UpdateCitizenResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public CitizenDto? Citizen { get; set; }
}
