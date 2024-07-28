using Asos.Exceptions;
using Asos.Interfaces;
using Asos.Models;
using Microsoft.Extensions.Options;

namespace Asos.Services;

public class LoginService(ITokenService tokenService, IOptions<JwtCredentials> jwtCredentials)
    : ILoginService
{
    private readonly JwtCredentials JwtCredentials = jwtCredentials.Value;

    public async Task<string?> LoginAsync(string username, string password)
    {
        if (username != JwtCredentials.Username || password != JwtCredentials.Password)
            throw new UnauthorizedException("Invalid credentials.");
        var token = await tokenService.GenerateJwtToken();
        return token;
    }
}