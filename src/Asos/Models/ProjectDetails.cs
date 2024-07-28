using Asos.Enums;
using Architecture = System.Runtime.InteropServices.Architecture;

namespace Asos.Models;

public class ProjectDetails
{
    public string ProjectName { get; set; } = default!;
    public string Creator { get; set; } = default!;
    public Language Language { get; set; }
    public Architecture Architecture { get; set; }
    public IFormFile Models { get; set; } = default!;
}