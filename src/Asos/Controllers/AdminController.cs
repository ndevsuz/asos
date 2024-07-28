using Asos.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asos.Controllers;

[Authorize]
[ApiController]
[Route("api/token")]
public class AdminController(ITokenService tokenService, ILoginService loginService) : BaseController
{
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateToken()
    {
        var token = await tokenService.GenerateToken();
        return Ok(token);
    }

    [AllowAnonymous]
    [HttpGet("login")]
    public async Task<IActionResult> LoginAsync(string username, string password)
        => Ok(await loginService.LoginAsync(username, password));
}