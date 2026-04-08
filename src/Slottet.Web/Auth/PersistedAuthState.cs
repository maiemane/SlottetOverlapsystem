namespace Slottet.Auth;

public sealed class PersistedAuthState
{
    public required AuthenticatedUser User { get; init; }
    public required string AccessToken { get; init; }
    public required string SelectedDepartment { get; init; }
    public required string SelectedShift { get; init; }
}
