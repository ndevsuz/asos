using Asos.Enums;
using Asos.Helpers;
using Asos.Interfaces;
using Asos.Models;
using Asos.TheStandart.CSharp.Services;

namespace Asos.Services;

public class ProjectAggregatorService(
    TheStandartCSharpProjectOrchestrationService theStandartCSharpProjectOrchestrationService)
{
    /// <summary>
    /// Create project with architecture and language
    /// </summary>
    /// <param name="projectDetails"></param>
    /// <returns>Returns zip folder path</returns>
    public string Create(ProjectDetails projectDetails)
    {
        var zipProjectPath = $"/home/FinishedProjects/{Guid.NewGuid()}.zip";

        switch (projectDetails.Architecture)
        {
            case Architecture.standart:
                switch (projectDetails.Language)
                {
                    case Language.csharp:
                        var projectFolderPath = theStandartCSharpProjectOrchestrationService.Initialize();
                        ZipCreator.Create(projectFolderPath, zipProjectPath);
                        break;
                }
                break;

            default:
                break;
        }

        return zipProjectPath;
    }
}