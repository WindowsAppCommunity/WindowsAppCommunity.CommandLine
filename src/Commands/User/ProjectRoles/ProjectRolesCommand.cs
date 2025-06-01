using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.User.ProjectRoles;

/// <summary>
/// Wacsdk project roles command for User.
/// </summary>
public class ProjectRolesCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectRolesCommand"/> class.
    /// </summary>
    public ProjectRolesCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> projectIdOption, Option<string> projectRoleIdOption, Option<string> roleIdOption, Option<string> roleNameOption, Option<string> roleDescriptionOption)
        : base("projects", "Manages the project roles of the User.")
    {
        AddCommand(new GetProjectRolesCommands(config, repoOption, idOption));
        AddCommand(new AddProjectRoleCommand(config, repoOption, idOption, projectIdOption, roleIdOption, roleNameOption, roleDescriptionOption));
        AddCommand(new RemoveProjectRoleCommand(config, repoOption, idOption, projectRoleIdOption));
    }
}