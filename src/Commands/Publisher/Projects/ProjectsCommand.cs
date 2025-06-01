using System.CommandLine;
using WindowsAppCommunity.CommandLine.Common.Projects;

namespace WindowsAppCommunity.CommandLine.Publisher.Projects;

/// <summary>
/// Wacsdk projects command for Publisher.
/// </summary>
public class ProjectsCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectsCommand"/> class.
    /// </summary>
    public ProjectsCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> projectIdOption)
        : base("projects", "Manages the projects of the Publisher.")
    {
        AddCommand(new GetProjectsCommands(config, repoOption, idOption));
        AddCommand(new AddProjectCommand(config, repoOption, idOption, projectIdOption));
        AddCommand(new RemoveProjectCommand(config, repoOption, idOption, projectIdOption));
    }
}