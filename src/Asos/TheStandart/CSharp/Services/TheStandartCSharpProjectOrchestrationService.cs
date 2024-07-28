using Asos.Models;
using Asos.Services;

namespace Asos.TheStandart.CSharp.Services;

public class TheStandartCSharpProjectOrchestrationService(ProjectDetails projectDetails)
{
    private TheStandartCSharpProjectService projectService;

    private string modelsFolderPath;
    private ModelService modelService;
    private string projectFolderPath;
    private string header;
    private string serverConnectionString;
    private string clientConnectionString;
    private string sourcePath;

    private void Setup()
    {
        modelsFolderPath = $"/home/ubuntu/tempmodels/{Guid.NewGuid()}";
        projectFolderPath = $"/home/ubuntu/projects/{Guid.NewGuid()}/{projectDetails.ProjectName}";
        sourcePath = $"/home/ubuntu/Sources/TheStandart/CSharp";

        Directory.CreateDirectory(modelsFolderPath);
        Directory.CreateDirectory(projectFolderPath);

        header = header = "// --------------------------------------------------------\r\n// " +
                "Copyright (c) Coalition of Good-Hearted Engineers\r\n// " +
                $"Developed by {projectDetails.Creator}\r\n// " +
                " --------------------------------------------------------";

        serverConnectionString = $"Host=localhost;Port=5432;Database={projectDetails.DatabaseName};Username=postgres;Password=olma";
        clientConnectionString = $"Host={projectDetails.Host};Port={projectDetails.Port};Database={projectDetails.DatabaseName};Username={projectDetails.UserName};Password={projectDetails.Password}";

        modelService = new ModelService();
        modelService.SplitModels(projectDetails.Models, modelsFolderPath);

        projectService = new TheStandartCSharpProjectService(
            projectName: projectDetails.ProjectName,
            modelsFolderPath: modelsFolderPath,
            projectFolderPath: projectFolderPath,
            header: this.header,
            serverConnectionString: this.serverConnectionString,
            clientConnectionString: this.clientConnectionString,
            sourcePath: this.sourcePath);

    }

    /// <summary>
    /// Initialize Standart Api
    /// </summary>
    /// <returns>Returns project folder path</returns>
    public string Initialize()
    {
        Setup();
        var projectsPath = projectService.CreateProjectStructure();
        projectService.InitializeBuildProject();

        TheStandartCSharpPackageManager.InstallPackagesToApiProject(projectsPath.ApiProjectPath);
        TheStandartCSharpPackageManager.InstallPackagesToTestProject(projectsPath.TestProjectPath);
        TheStandartCSharpPackageManager.InstallPackagesToBuildProject(projectsPath.BuildProjectPath);

        projectService.CreateModels();
        projectService.CreateExceptions();
        projectService.CreateBrokers();
        projectService.WriteAppSettings(serverConnectionString);
        projectService.CreateServices();
        projectService.CreateControllers();
        projectService.ConfigureProject();
        projectService.WriteTests();
        projectService.WriteAppSettings(clientConnectionString);
        projectService.BuildBuildProject();

        return projectFolderPath;
    }
}
