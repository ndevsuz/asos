using Asos.Enums;

namespace Asos.Models;

public class ProjectDetails
{
    public string ProjectName { get; set; } = default!;
    public string Creator { get; set; } = default!;
    public Language Language { get; set; }
    public Architecture Architecture { get; set; }
    public IFormFile Models { get; set; } = default!;
    public string Host { get; set; } = default!;
    public string Port { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string DatabaseName { get; set; } = default!;
}