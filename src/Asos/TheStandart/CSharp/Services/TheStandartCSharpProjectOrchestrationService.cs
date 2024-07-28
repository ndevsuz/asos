using Asos.Models;
using Asos.Service;

namespace Asos.TheStandart.CSharp.Services;

public class TheStandartCSharpProjectOrchestrationService(ProjectDetails projectDetails)
{
    private TheStandartCSharpProjectService projectService;
    private ProjectDetails projectDetails;
    private ModelService modelService;

    private string modelsFolderPath;
    private string projectFolderPath;
    private string header;
    private string serverConnectionString;
    private string clientConnectionString;
    private string sourcePath;

    private void Setup()
    {
        modelsFolderPath = $"/home/tempmodels/{Guid.NewGuid()}";
        projectFolderPath = $"/home/projects/{Guid.NewGuid()}/{projectDetails.ProjectName}";

        Directory.CreateDirectory(modelsFolderPath);
        Directory.CreateDirectory(projectFolderPath);

        header = header = "// --------------------------------------------------------\r\n// " +
                "Copyright (c) Coalition of Good-Hearted Engineers\r\n// " +
                $"Developed by {projectDetails.Creator}\r\n// " +
                " --------------------------------------------------------";

        serverConnectionString = $"Host=localhost;Port=5432;Database={projectDetails.DatabaseName};Username=postgres;Password=olma";
        clientConnectionString = $"Host={projectDetails.Host};Port={projectDetails.Port};Database={projectDetails.DatabaseName};Username={projectDetails.UserName};Password={projectDetails.Password}";


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

    public void Initialize()
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
        projectService.CreateServices();
        projectService.CreateControllers();
        projectService.ConfigureProject();
        projectService.WriteTests();
        projectService.BuildBuildProject();
    }
}
