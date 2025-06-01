using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.User.ExtendedDescription;

/// <summary>
/// Wacsdk entity extended description command for User.
/// </summary>
public class EntityExtendedDescription : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityExtendedDescription"/> class.
    /// </summary>
    public EntityExtendedDescription(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> valueOption)
        : base("extended-description", "Manages the extended description of the User.")
    {
        AddCommand(new GetEntityExtendedDescriptionCommand(config, repoOption, idOption));
        AddCommand(new SetEntityExtendedDescriptionCommand(config, repoOption, idOption, valueOption));
    }
}