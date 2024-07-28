using Asos.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asos.Controllers;

[ApiController]
[Authorize]
[Route("api/token")]
public class AdminController(ITokenService tokenService, ILoginService loginService) : BaseController
{
    [AllowAnonymous]
    [HttpGet("login")]
    public async Task<IActionResult> LoginAsync(string username, string password)
        => Ok(await loginService.LoginAsync(username, password));

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateTokenAsync()
    {
        var token = await tokenService.GenerateToken();
        return Ok(token);
    }

    [HttpGet("get-all")]
    public async Task<IActionResult> GetAllAsync()
        => Ok(await tokenService.GetAllAsync());
}