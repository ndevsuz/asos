using System.IdentityModel.Tokens.Jwt;
using Asos.Interfaces;
using System.Security.Cryptography;
using System.Text;
using Asos.Exceptions;
using Asos.Helpers;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace Asos.Services;

public class TokenService(IConnectionMultiplexer redis, IConfiguration configuration) : ITokenService
{
    private readonly IConfiguration _config = configuration.GetSection("JWT");
     
    public async Task<string> GenerateToken()
    {
        var randomNumber = new byte[32]; // 256 bits
        using (var rn = RandomNumberGenerator.Create())
        {
            rn.GetBytes(randomNumber);
        }

        var token = Convert.ToBase64String(randomNumber);
        var db = redis.GetDatabase();
        await db.StringSetAsync(token, "valid");
        return token;
    }

    public async Task<bool> GetToken(string token)
    {
        var db = redis.GetDatabase();
        var tokenValue = await db.StringGetAsync(token);
        
        if (string.IsNullOrEmpty(tokenValue))
        {
            throw new UnauthorizedException("Invalid token.");
        }
        
        if (tokenValue != "valid")
        {
            throw new UnauthorizedException("Token is already used or invalid.");
        }

        await db.StringSetAsync(token, "used");

        return true;
    }

    public Task<string> GenerateJwtToken()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["SecurityKey"]!));
        var keyCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresHours = int.Parse(_config["Lifetime"]!);
        var token = new JwtSecurityToken(
            issuer: _config["Issuer"],
            audience: _config["Audience"],
            expires: TimeConstants.GetNow().AddHours(expiresHours),
            signingCredentials: keyCredentials );
        
        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));

    }
    
}