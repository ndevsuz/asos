using Asos.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asos.Controllers;

[Route("api/token")]
[ApiController]
public class AdminController(ITokenService tokenService, ILoginService loginService) : BaseController
{
    [HttpPost("generate")]
    [Authorize]
    public async Task<IActionResult> GenerateToken()
    {
        var token = await tokenService.GenerateToken();
        return Ok(token);
    }
    
    [HttpGet("login")]
    public async Task<IActionResult> LoginAsync(string username, string password)
        => Ok(await loginService.LoginAsync(username, password));
}