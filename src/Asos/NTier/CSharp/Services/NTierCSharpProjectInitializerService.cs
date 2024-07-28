using Asos.Helpers;
using Asos.NTier.CSharp.Models;

namespace Asos.NTier.CSharp.Services
{
    public class NTierCSharpProjectInitializerService(string projectName,
        IHostEnvironment hostEnvironment = null
        )
    {
        public ProjectsPath Initialize()
        {
            // var wwwroot = hostEnvironment.ContentRootPath;
            var wwwroot = "wwwroot";
            var tempFolderPath = Path.Combine("../TempProjects", Guid.NewGuid().ToString());

            if (!Directory.Exists(tempFolderPath))
                Directory.CreateDirectory(tempFolderPath);

            var projectFolderPath = Path.Combine(tempFolderPath, projectName);

            if (!Directory.Exists(projectFolderPath))
                Directory.CreateDirectory(projectFolderPath);

            //Create solution
            Process.ExecuteCommand($"dotnet new sln --name {projectName} --output {projectFolderPath}");
            var solutionPath = Path.Combine(projectFolderPath, $"{projectName}.sln");

            //Create src folder
            var srcFolderPath = Path.Combine(projectFolderPath, "src");
            Directory.CreateDirectory(srcFolderPath);

            var domain = Path.Combine(srcFolderPath, $"{projectName}.Domain");
            var dataAccess = Path.Combine(srcFolderPath, $"{projectName}.DataAccess");
            var service = Path.Combine(srcFolderPath, $"{projectName}.Service");
            var api = Path.Combine(srcFolderPath, $"{projectName}.Api");

            var domainName = $"{projectName}.Domain";
            var dataAccessName = $"{projectName}.DataAccess";
            var serviceName = $"{projectName}.Service";
            var apiName = $"{projectName}.Api";

            Process.ExecuteCommand($"dotnet new classlib --name {domainName} --output {Path.Combine(srcFolderPath, domainName)}");
            Process.ExecuteCommand($"dotnet sln {solutionPath} add {Path.Combine(domain, $"{domainName}.csproj")}");

            Process.ExecuteCommand($"dotnet new classlib --name {dataAccessName} --output {Path.Combine(srcFolderPath, dataAccessName)}");
            Process.ExecuteCommand($"dotnet sln {solutionPath} add {Path.Combine(dataAccess, $"{dataAccessName}.csproj")}");

            Process.ExecuteCommand($"dotnet new classlib --name {serviceName} --output {Path.Combine(srcFolderPath, serviceName)}");
            Process.ExecuteCommand($"dotnet sln {solutionPath} add {Path.Combine(service, $"{serviceName}.csproj")}");

            Process.ExecuteCommand($"dotnet new webapi --name {apiName} --output {Path.Combine(srcFolderPath, apiName)}");
            Process.ExecuteCommand($"dotnet sln {solutionPath} add {Path.Combine(api, $"{apiName}.csproj")}");

            //add references
            Process.ExecuteCommand($"dotnet add {Path.Combine(dataAccess, $"{dataAccessName}.csproj")} reference {Path.Combine(domain, $"{domainName}.csproj")}");
            Process.ExecuteCommand($"dotnet add {Path.Combine(service, $"{serviceName}.csproj")} reference {Path.Combine(dataAccess, $"{dataAccessName}.csproj")}");
            Process.ExecuteCommand($"dotnet add {Path.Combine(api, $"{apiName}.csproj")} reference {Path.Combine(service, $"{serviceName}.csproj")}");

            return new ProjectsPath
            {
                SrcPath = srcFolderPath,
                SolutionPath = solutionPath,
                DomainProjectPath = domain,
                DataAccessProjectPath = dataAccess,
                ServiceProjectPath = service,
                ApiProjectPath = api,
            };
        }
    }
}
