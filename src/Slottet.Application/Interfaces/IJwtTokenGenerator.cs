using Slottet.Application.DTOs.Auth;
using Slottet.Domain.Entities;

namespace Slottet.Application.Interfaces;

public interface IJwtTokenGenerator
{
    JwtTokenResult CreateToken(Employee employee);
}