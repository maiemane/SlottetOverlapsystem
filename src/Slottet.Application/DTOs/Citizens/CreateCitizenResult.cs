namespace Slottet.Application.DTOs.Citizens;

public sealed class CreateCitizenResult
{
    public bool IsSuccess { get; set; }
    public string? Error { get; set; }
    public CreateCitizenResponse? Citizen { get; set; }
}
