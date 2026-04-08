namespace Slottet.Auth;

public interface IAuthSessionStore
{
    Task<PersistedAuthState?> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(PersistedAuthState state, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}
