using Asos.Interfaces;
using Asos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asos.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AdminController(ITokenService tokenService, ILoginService loginService) : BaseController
{
    [AllowAnonymous]
    [HttpGet("login")]
    public async Task<IActionResult> LoginAsync(string username, string password)
        => Ok(new Response()
        {
            StatusCode = 200,
            Message = "Succes!",
            Data = await loginService.LoginAsync(username, password)

        });

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateTokenAsync()
    {
        var token = await tokenService.GenerateToken();
        return Ok(new Response()
        {
            StatusCode = 200,
            Message = "Succes!",
            Data = token

        });
    }

    [HttpGet("get-all")]
    public async Task<IActionResult> GetAllAsync()
        => Ok(new Response()
        {
            StatusCode = 200,
            Message = "Succes!",
            Data = await tokenService.GetAllAsync()

        });
}