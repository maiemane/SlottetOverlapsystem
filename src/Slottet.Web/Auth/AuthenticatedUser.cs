namespace Slottet.Auth;

public sealed class AuthenticatedUser
{
    public required string Email { get; init; }
    public required string FullName { get; init; }
    public required string Role { get; init; }
}
