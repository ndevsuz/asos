using System.IdentityModel.Tokens.Jwt;
using Asos.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Asos.Dtos;
using Asos.Exceptions;
using Asos.Helpers;
using Asos.Models;
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
        var tokenCreationDto = new TokenCreationDto
        {
            Token = token,
            Status = "valid",
            CreatedDate = DateTime.UtcNow,
            UsedDate = null
        };

        var serializedToken = JsonSerializer.Serialize(tokenCreationDto);
        var db = redis.GetDatabase();
        await db.StringSetAsync(token, serializedToken);
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

        var tokenCreationDto = JsonSerializer.Deserialize<TokenCreationDto>(tokenValue);

        if (tokenCreationDto is not { Status: "valid" })
        {
            throw new UnauthorizedException("Token is already used or invalid.");
        }

        tokenCreationDto.Status = "used";
        tokenCreationDto.UsedDate = DateTime.UtcNow;

        var serializedToken = JsonSerializer.Serialize(tokenCreationDto);
        await db.StringSetAsync(token, serializedToken);

        return true;
    }
    
    public async Task<IList<TokenDetails>> GetAllAsync()
    {
        var db = redis.GetDatabase();
        var server = redis.GetServer(redis.GetEndPoints()[0]);
        var keys = server.Keys();

        var tokenList = new List<TokenDetails>();

        foreach (var key in keys)
        {
            var tokenValue = await db.StringGetAsync(key);
            if (tokenValue.IsNullOrEmpty) continue;
            var tokenDetails = JsonSerializer.Deserialize<TokenDetails>(tokenValue);
            tokenList.Add(tokenDetails);
        }

        return tokenList;
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