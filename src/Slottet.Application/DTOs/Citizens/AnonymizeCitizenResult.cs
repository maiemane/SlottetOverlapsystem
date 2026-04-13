namespace Slottet.Application.DTOs.Citizens;

public sealed class AnonymizeCitizenResult
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
}
