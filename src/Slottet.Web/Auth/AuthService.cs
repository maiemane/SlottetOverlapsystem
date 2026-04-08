using System.Net;
using System.Net.Http.Json;

namespace Slottet.Auth;

public sealed class AuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuthSessionStore _authSessionStore;
    private readonly IReadOnlyList<string> _departments = ["Slottet", "Skoven"];
    private readonly IReadOnlyList<string> _shifts = ["Dag", "Aften", "Nat"];
    private bool _isInitialized;
    private Task? _initializationTask;

    public AuthService(IHttpClientFactory httpClientFactory, IAuthSessionStore authSessionStore)
    {
        _httpClientFactory = httpClientFactory;
        _authSessionStore = authSessionStore;
    }

    public AuthenticatedUser? CurrentUser { get; private set; }
    public string? AccessToken { get; private set; }
    public string SelectedDepartment { get; private set; } = "Slottet";
    public string SelectedShift { get; private set; } = "Dag";
    public bool IsAuthenticated => CurrentUser is not null && !string.IsNullOrWhiteSpace(AccessToken);
    public bool IsInitialized => _isInitialized;
    public event Action? AuthenticationStateChanged;

    public IReadOnlyList<string> Departments => _departments;
    public IReadOnlyList<string> Shifts => _shifts;

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
        {
            return Task.CompletedTask;
        }

        _initializationTask ??= InitializeCoreAsync(cancellationToken);
        return _initializationTask;
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("SlottetApi");

            using var response = await client.PostAsJsonAsync("api/auth/login", new LoginRequest
            {
                Email = email.Trim(),
                Password = password
            }, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return (false, "Login mislykkedes. Tjek email og password.");
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var badRequestMessage = await response.Content.ReadAsStringAsync(cancellationToken);
                return (false, string.IsNullOrWhiteSpace(badRequestMessage) ? "Ugyldig loginanmodning." : badRequestMessage);
            }

            if (!response.IsSuccessStatusCode)
            {
                return (false, "Kunne ikke kontakte login-servicen.");
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken: cancellationToken);

            if (loginResponse is null)
            {
                return (false, "Login-svaret fra API'et var ugyldigt.");
            }

            CurrentUser = new AuthenticatedUser
            {
                Email = loginResponse.Email,
                FullName = loginResponse.Name,
                Role = loginResponse.Role,
                ExpiresAtUtc = loginResponse.ExpiresAtUtc
            };
            AccessToken = loginResponse.AccessToken;
            SelectedDepartment = GetDefaultDepartment(loginResponse.Role);
            SelectedShift = GetDefaultShift(loginResponse.Role);

            await PersistStateAsync(cancellationToken);
            AuthenticationStateChanged?.Invoke();
            return (true, null);
        }
        catch (TaskCanceledException)
        {
            return (false, "Login-anmodningen udløb. Prøv igen.");
        }
        catch (HttpRequestException)
        {
            return (false, "Kunne ikke oprette forbindelse til API'et. Tjek at API'et kører og at HTTPS-certifikatet virker.");
        }
        catch (Exception)
        {
            return (false, "Der opstod en uventet fejl under login.");
        }
    }

    public void SetSelectedDepartment(string department)
    {
        if (!_departments.Contains(department, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        var resolvedDepartment = _departments.First(x => string.Equals(x, department, StringComparison.OrdinalIgnoreCase));

        if (SelectedDepartment == resolvedDepartment)
        {
            return;
        }

        SelectedDepartment = resolvedDepartment;
        _ = PersistStateSafeAsync();
        AuthenticationStateChanged?.Invoke();
    }

    public void SetSelectedShift(string shift)
    {
        if (!_shifts.Contains(shift, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        var resolvedShift = _shifts.First(x => string.Equals(x, shift, StringComparison.OrdinalIgnoreCase));

        if (SelectedShift == resolvedShift)
        {
            return;
        }

        SelectedShift = resolvedShift;
        _ = PersistStateSafeAsync();
        AuthenticationStateChanged?.Invoke();
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        ClearState();
        await _authSessionStore.ClearAsync(cancellationToken);
        AuthenticationStateChanged?.Invoke();
    }

    private static string GetDefaultDepartment(string role)
    {
        return role == "Medarbejder" ? "Slottet" : "Slottet";
    }

    private static string GetDefaultShift(string role)
    {
        return role == "Medarbejder" ? "Dag" : "Dag";
    }

    private async Task InitializeCoreAsync(CancellationToken cancellationToken)
    {
        var persistedState = await _authSessionStore.LoadAsync(cancellationToken);

        if (persistedState is not null)
        {
            var isExpired = persistedState.User.ExpiresAtUtc <= DateTime.UtcNow;

            if (isExpired)
            {
                await _authSessionStore.ClearAsync(cancellationToken);
            }
            else
            {
                CurrentUser = persistedState.User;
                AccessToken = persistedState.AccessToken;
                SelectedDepartment = ResolveDepartment(persistedState.SelectedDepartment);
                SelectedShift = ResolveShift(persistedState.SelectedShift);
            }
        }

        _isInitialized = true;
        AuthenticationStateChanged?.Invoke();
    }

    private Task PersistStateAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAuthenticated || CurrentUser is null || string.IsNullOrWhiteSpace(AccessToken))
        {
            return _authSessionStore.ClearAsync(cancellationToken);
        }

        return _authSessionStore.SaveAsync(new PersistedAuthState
        {
            User = CurrentUser,
            AccessToken = AccessToken,
            SelectedDepartment = SelectedDepartment,
            SelectedShift = SelectedShift
        }, cancellationToken);
    }

    private async Task PersistStateSafeAsync()
    {
        try
        {
            await PersistStateAsync();
        }
        catch
        {
            // Ignore storage write failures and keep the in-memory auth state alive.
        }
    }

    private void ClearState()
    {
        CurrentUser = null;
        AccessToken = null;
        SelectedDepartment = "Slottet";
        SelectedShift = "Dag";
    }

    private string ResolveDepartment(string department)
    {
        return _departments.FirstOrDefault(x => string.Equals(x, department, StringComparison.OrdinalIgnoreCase))
            ?? "Slottet";
    }

    private string ResolveShift(string shift)
    {
        return _shifts.FirstOrDefault(x => string.Equals(x, shift, StringComparison.OrdinalIgnoreCase))
            ?? "Dag";
    }
}
