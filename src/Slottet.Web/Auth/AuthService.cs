using System.Net;
using System.Net.Http.Json;

namespace Slottet.Auth;

public sealed class AuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IReadOnlyList<string> _departments = ["Slottet", "Skoven"];
    private readonly IReadOnlyList<string> _shifts = ["Dag", "Aften", "Nat"];

    public AuthService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public AuthenticatedUser? CurrentUser { get; private set; }
    public string? AccessToken { get; private set; }
    public string SelectedDepartment { get; private set; } = "Slottet";
    public string SelectedShift { get; private set; } = "Dag";
    public bool IsAuthenticated => CurrentUser is not null && !string.IsNullOrWhiteSpace(AccessToken);
    public event Action? AuthenticationStateChanged;

    public IReadOnlyList<string> Departments => _departments;
    public IReadOnlyList<string> Shifts => _shifts;

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
        AuthenticationStateChanged?.Invoke();
    }

    public void Logout()
    {
        CurrentUser = null;
        AccessToken = null;
        SelectedDepartment = "Slottet";
        SelectedShift = "Dag";
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
}
