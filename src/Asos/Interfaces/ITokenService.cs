using Asos.Dtos;
using Asos.Models;

namespace Asos.Interfaces;

public interface ITokenService
{
    Task<string> GenerateToken();
    Task<bool> GetToken(string token);
    Task<string> GenerateJwtToken();
    Task<IList<TokenDetails>> GetAllAsync();
}