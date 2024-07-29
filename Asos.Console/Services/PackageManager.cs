
namespace Easy.Services
{
    public static class PackageManager
    {
        public static void InstallPackagesToApiProject(string apiProjectPath)
        {
            Process.ExecuteCommand($"dotnet add {apiProjectPath} package EFxceptions --version 0.4.5", "Installing... EFxceptions");
            Process.ExecuteCommand($"dotnet add {apiProjectPath} package Microsoft.AspNetCore.Cors --version 2.2.0", "Installing... Microsoft.AspnetCore.Cors");
            Process.ExecuteCommand($"dotnet add {apiProjectPath} package Microsoft.AspNetCore.OData --version 8.2.0", "Installing... Microsoft.AspNetCore.OData");
            Process.ExecuteCommand($"dotnet add {apiProjectPath} package Microsoft.EntityFrameworkCore --version 8.0.6", "Installing... Microsoft.EntityFrameworkCore");
            Process.ExecuteCommand($"dotnet add {apiProjectPath} package Microsoft.EntityFrameworkCore.Design --version 8.0.6", "Installing... EfCore Design");
            Process.ExecuteCommand($"dotnet add {apiProjectPath} package Microsoft.EntityFrameworkCore.Relational --version 8.0.6", "Installing... Microsoft.EntityFrameworkCore.Relational");
            Process.ExecuteCommand($"dotnet add {apiProjectPath} package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.4", "Installing... Npgsql.EntityFrameworkCore.PostgreSQL");
            Process.ExecuteCommand($"dotnet add {apiProjectPath} package Microsoft.EntityFrameworkCore.Tools --version 8.0.6", "Installing... Microsoft.EntityFrameworkCore.Tools");
            Process.ExecuteCommand($"dotnet add {apiProjectPath} package RESTFulSense --version 3.0.0", "Installing... RESTFulSense");
            Process.ExecuteCommand($"dotnet add {apiProjectPath} package Swashbuckle.AspNetCore --version 6.6.2", "Installing... Swashbuckle.AspNetCore");
            Process.ExecuteCommand($"dotnet add {apiProjectPath} package Xeption --version 2.6.0", "Installing... Xeption");
        }

        public static void InstallPackagesToBuildProject(string buildProjectPath)
        {
            Process.ExecuteCommand($"dotnet add {buildProjectPath} package ADotNet --version 3.0.3", "Installing... ADotNet");
        }

        internal static void InstallPackagesToTestProject(string testProjectPath)
        {
            Process.ExecuteCommand($"dotnet add {testProjectPath} package coverlet.collector --version 6.0.2", "Installing... coverlet.collector");
            Process.ExecuteCommand($"dotnet add {testProjectPath} package DeepCloner --version 0.10.4", "Installing... DeepCloner");
            Process.ExecuteCommand($"dotnet add {testProjectPath} package FluentAssertions --version 6.12.0", "Installing... FluentAssertions");
            Process.ExecuteCommand($"dotnet add {testProjectPath} package Microsoft.NET.Test.Sdk --version 17.10.0", "Installing... Microsoft.NET.Tests.Sdk");
            Process.ExecuteCommand($"dotnet add {testProjectPath} package Moq --version 4.20.70", "Installing... Moq");
            Process.ExecuteCommand($"dotnet add {testProjectPath} package Tynamix.ObjectFiller --version 1.5.8", "Installing... Tynamix.ObjectFiller");
            Process.ExecuteCommand($"dotnet add {testProjectPath} package xunit --version 2.8.1", "Installing... xunit");
            Process.ExecuteCommand($"dotnet add {testProjectPath} package xunit.runner.visualstudio --version 2.8.1", "Installing... xunit.runner.visualstudio");
        }
    }
}
