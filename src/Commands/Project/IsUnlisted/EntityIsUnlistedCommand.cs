using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.Project.IsUnlisted;

/// <summary>
/// Wacsdk entity is unlisted command for Project.
/// </summary>
public class EntityIsUnlistedCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityIsUnlistedCommand"/> class.
    /// </summary>
    public EntityIsUnlistedCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<bool> valueOption)
        : base("is-unlisted", "Manages the is unlisted flag of the Project.")
    {
        AddCommand(new GetEntityIsUnlistedCommand(config, repoOption, idOption));
        AddCommand(new SetEntityIsUnlistedCommand(config, repoOption, idOption, valueOption));
    }
}