using Asos.Exceptions;
using Asos.Interfaces;
using Asos.Interfaces;
using Asos.Models;
using Microsoft.AspNetCore.Mvc;

namespace Asos.Controllers;

[ApiController]
[Route("api/[controller]")] // iskandaraka zb qimasdan oziz xolaganizcha qb qoyarsz
public class UsersController(ITokenService tokenService) : BaseController
{
    [HttpPost("validate")]
    public async Task<IActionResult> GetToken(string token, ProjectDetails projectDetails)
    {
        var validToken = await tokenService.GetToken(token);
        return Ok(validToken);
    }
}