using Slottet.Application.DTOs.Auth;

namespace Slottet.Application.Interfaces;



public interface ILoginService
{
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}