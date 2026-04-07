namespace Slottet.Api.Auth;

public sealed class AuthTokenResult
{
    public required string AccessToken { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }
}
