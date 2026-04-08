using System.Text.Json;
using Microsoft.JSInterop;

namespace Slottet.Auth;

public sealed class BrowserSessionAuthStore : IAuthSessionStore
{
    private const string StorageKey = "slottet.auth";
    private readonly IJSRuntime _jsRuntime;

    public BrowserSessionAuthStore(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<PersistedAuthState?> LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("slottetAuthStorage.get", cancellationToken, StorageKey);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<PersistedAuthState>(json);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (JSDisconnectedException)
        {
            return null;
        }
        catch (JsonException)
        {
            await ClearAsync(cancellationToken);
            return null;
        }
    }

    public Task SaveAsync(PersistedAuthState state, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(state);
        return _jsRuntime.InvokeVoidAsync("slottetAuthStorage.set", cancellationToken, StorageKey, json).AsTask();
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        return _jsRuntime.InvokeVoidAsync("slottetAuthStorage.remove", cancellationToken, StorageKey).AsTask();
    }
}
