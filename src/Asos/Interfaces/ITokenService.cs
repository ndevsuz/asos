namespace Asos.Interfaces;

public interface ITokenService
{
    Task<string> GenerateToken();
    Task<bool> GetToken(string token);
    Task<string> GenerateJwtToken();
}