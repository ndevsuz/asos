using Asos.Interfaces;
using Asos.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Asos.Controllers;

[ApiController]
[Route("api/[controller]")] // iskandaraka zb qimasdan oziz xolaganizcha qb qoyarsz
public class UsersController(ITokenService tokenService) : BaseController
{
    [HttpPost("validate")]
    public async Task<IActionResult> GetToken([FromBody] string token)
    {
        var validToken = await tokenService.GetToken(token);
        return Ok(validToken);
    }
}