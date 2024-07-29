using System.Text;

namespace Easy.Services
{
    public class AsosService
    {
        private string projectName;
        private string modelsFolderPath;
        private string projectFolderPath;
        private string header;
        private string connectionString;

        private ProjectService projectService;

        public AsosService(
            string projectName,
            string modelsFolderPath,
            string projectFolderPath,
            string header,
            string connectionString)
        {
            this.projectName = projectName;
            this.modelsFolderPath = modelsFolderPath;
            this.projectFolderPath = projectFolderPath;
            this.header = header;
            this.connectionString = connectionString;

            projectService = new ProjectService(
                projectName: projectName,
                modelsFolderPath: modelsFolderPath,
                projectFolderPath: projectFolderPath,
                header: header,
                connectionString: connectionString);
        }

        public void Initialize()
        {
            var projectsPath = projectService.CreateProjectStructure();

            Console.WriteLine("Creating projects...");
            projectService.InitializeBuildProject();

            Console.WriteLine("Installing packages...");
            PackageManager.InstallPackagesToApiProject(projectsPath.ApiProjectPath);
            PackageManager.InstallPackagesToBuildProject(projectsPath.BuildProjectPath);
            PackageManager.InstallPackagesToTestProject(projectsPath.TestProjectPath);

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
}