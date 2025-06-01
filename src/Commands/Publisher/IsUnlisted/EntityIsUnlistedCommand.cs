using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.Publisher.IsUnlisted;

/// <summary>
/// Wacsdk entity is unlisted command for Publisher.
/// </summary>
public class EntityIsUnlistedCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityIsUnlistedCommand"/> class.
    /// </summary>
    public EntityIsUnlistedCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<bool> valueOption)
        : base("isunlisted", "Manages the is unlisted status of the Publisher.")
    {
        AddCommand(new GetEntityIsUnlistedCommand(config, repoOption, idOption));
        AddCommand(new SetEntityIsUnlistedCommand(config, repoOption, idOption, valueOption));
    }
}