using Asos.Exceptions;
using Asos.Interfaces;
using Asos.Interfaces;
using Asos.Models;
using Asos.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;

namespace Asos.Controllers;

[ApiController]
[Route("api/[controller]")] // iskandaraka zb qimasdan oziz xolaganizcha qb qoyarsz
public class UsersController : BaseController
{
    private ITokenService tokenService;
    private ProjectAggregatorService projectAggregatorService;

    public UsersController(ITokenService tokenService)
    {
        this.tokenService = tokenService;
    }


    [HttpPost("generate")]
    public async Task<IActionResult> GetToken(string token, ProjectDetails projectDetails)
    {
        bool validToken;

        if (token == "admin123")
            validToken = true;
        else
            validToken = await tokenService.GetToken(token);

        this.projectAggregatorService = new ProjectAggregatorService(
            theStandartCSharpProjectOrchestrationService:
                new TheStandart.CSharp.Services.TheStandartCSharpProjectOrchestrationService(projectDetails));

        var zipFilePath = projectAggregatorService.Create(projectDetails);

        return Ok(new Response
        {
            StatusCode = 200,
            Message = "Success",
            Data = zipFilePath
        });
    }
}