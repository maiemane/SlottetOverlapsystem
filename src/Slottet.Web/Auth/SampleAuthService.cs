namespace Slottet.Auth;

public sealed class SampleAuthService
{
    private readonly IReadOnlyList<string> _departments = ["Slottet", "Skoven"];

    private readonly IReadOnlyList<SampleUserAccount> _sampleUsers =
    [
        new SampleUserAccount
        {
            Email = "medarbejder@slottet.dk",
            Password = "med123",
            FullName = "Maja Jensen",
            Role = "Medarbejder"
        },
        new SampleUserAccount
        {
            Email = "admin@slottet.dk",
            Password = "admin123",
            FullName = "Jonas Andersen",
            Role = "Admin"
        }
    ];

    public AuthenticatedUser? CurrentUser { get; private set; }

    public string SelectedDepartment { get; private set; } = "Slottet";

    public bool IsAuthenticated => CurrentUser is not null;

    public event Action? AuthenticationStateChanged;

    public IReadOnlyList<SampleUserAccount> SampleUsers => _sampleUsers;

    public IReadOnlyList<string> Departments => _departments;

    public bool TryLogin(string email, string password, out string? errorMessage)
    {
        var user = _sampleUsers.FirstOrDefault(sampleUser =>
            string.Equals(sampleUser.Email, email.Trim(), StringComparison.OrdinalIgnoreCase) &&
            sampleUser.Password == password);

        if (user is null)
        {
            errorMessage = "Login mislykkedes. Tjek email og password.";
            return false;
        }

        CurrentUser = new AuthenticatedUser
        {
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role
        };
        SelectedDepartment = GetDefaultDepartment(user.Role);

        errorMessage = null;
        AuthenticationStateChanged?.Invoke();
        return true;
    }

    public void SetSelectedDepartment(string department)
    {
        if (!_departments.Contains(department, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        if (string.Equals(SelectedDepartment, department, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        SelectedDepartment = _departments.First(value =>
            string.Equals(value, department, StringComparison.OrdinalIgnoreCase));
        AuthenticationStateChanged?.Invoke();
    }

    public void Logout()
    {
        if (CurrentUser is null)
        {
            return;
        }

        CurrentUser = null;
        SelectedDepartment = "Slottet";
        AuthenticationStateChanged?.Invoke();
    }

    private static string GetDefaultDepartment(string role)
    {
        return role == "Medarbejder" ? "Slottet" : "Slottet";
    }
}
