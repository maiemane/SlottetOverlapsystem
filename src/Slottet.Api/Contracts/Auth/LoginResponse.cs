namespace Slottet.Api.Contracts.Auth;

public sealed class LoginResponse
{
    public required string AccessToken { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string Role { get; init; }
}
