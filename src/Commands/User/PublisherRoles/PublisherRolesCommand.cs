using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.User.PublisherRoles;

/// <summary>
/// Wacsdk publisher roles command for User.
/// </summary>
public class PublisherRolesCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublisherRolesCommand"/> class.
    /// </summary>
    public PublisherRolesCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> publisherIdOption, Option<string> publisherRoleIdOption, Option<string> roleIdOption, Option<string> roleNameOption, Option<string> roleDescriptionOption)
        : base("publishers", "Manages the publisher roles of the User.")
    {
        AddCommand(new GetPublisherRolesCommands(config, repoOption, idOption));
        AddCommand(new AddPublisherRoleCommand(config, repoOption, idOption, publisherIdOption, roleIdOption, roleNameOption, roleDescriptionOption));
        AddCommand(new RemovePublisherRoleCommand(config, repoOption, idOption, publisherRoleIdOption));
    }
}