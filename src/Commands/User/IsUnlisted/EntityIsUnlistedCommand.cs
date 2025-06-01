using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.User.IsUnlisted;

/// <summary>
/// Wacsdk entity is unlisted command for User.
/// </summary>
public class EntityIsUnlistedCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityIsUnlistedCommand"/> class.
    /// </summary>
    public EntityIsUnlistedCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<bool> valueOption)
        : base("is-unlisted", "Manages the is unlisted setting of the User.")
    {
        AddCommand(new GetEntityIsUnlistedCommand(config, repoOption, idOption));
        AddCommand(new SetEntityIsUnlistedCommand(config, repoOption, idOption, valueOption));
    }
}