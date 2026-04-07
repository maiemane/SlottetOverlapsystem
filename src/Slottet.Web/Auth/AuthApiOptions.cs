namespace Slottet.Auth;

public sealed class AuthApiOptions
{
    public const string SectionName = "Api";

    public string BaseUrl { get; set; } = string.Empty;
}
