namespace Slottet.Application.DTOs.Citizens;

public sealed class DeleteCitizenResult
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
}
