using Asos.Helpers;
using Asos.NTier.CSharp.Models;

namespace Asos.NTier.CSharp.Services;

public class NTierCSharpPackageInstallerService
{
    public void InstallAllPackages(ProjectsPath projectsPath)
    {
        Process.ExecuteCommand($"dotnet add {projectsPath.DataAccessProjectPath} package Microsoft.EntityFrameworkCore.Tools --version 8.0.4");
        Process.ExecuteCommand($"dotnet add {projectsPath.DataAccessProjectPath} package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.4");
        Process.ExecuteCommand($"dotnet add {projectsPath.ApiProjectPath} package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.4");
        Process.ExecuteCommand($"dotnet add {projectsPath.ServiceProjectPath} package AutoMapper --version 13.0.1");
    }
}
