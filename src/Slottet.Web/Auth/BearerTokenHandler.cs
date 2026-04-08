using System.Net.Http.Headers;

namespace Slottet.Auth;

public sealed class BearerTokenHandler : DelegatingHandler
{
    private static readonly PathString LoginPath = new("/api/auth/login");
    private readonly AuthService _authService;

    public BearerTokenHandler(AuthService authService)
    {
        _authService = authService;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri is not null &&
            !LoginPath.Equals(request.RequestUri.AbsolutePath, StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(_authService.AccessToken) &&
            request.Headers.Authorization is null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authService.AccessToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
