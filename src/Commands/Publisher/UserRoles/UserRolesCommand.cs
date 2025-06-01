using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.Publisher.UserRoles;

/// <summary>
/// Wacsdk user roles command for Publisher.
/// </summary>
public class UserRolesCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserRolesCommand"/> class.
    /// </summary>
    public UserRolesCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> userIdOption, Option<string> roleIdOption, Option<string> roleNameOption, Option<string> roleDescriptionOption)
        : base("users", "Manages the user roles of the Publisher.")
    {
        AddCommand(new GetUserRolesCommands(config, repoOption, idOption));
        AddCommand(new AddUserRoleCommand(config, repoOption, idOption, userIdOption, roleIdOption, roleNameOption, roleDescriptionOption));
        AddCommand(new RemoveUserRoleCommand(config, repoOption, idOption, userIdOption));
    }
}