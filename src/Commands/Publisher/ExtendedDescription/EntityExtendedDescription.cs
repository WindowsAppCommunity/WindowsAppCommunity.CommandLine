using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.Publisher.ExtendedDescription;

/// <summary>
/// Wacsdk entity extended description command for Publisher.
/// </summary>
public class EntityExtendedDescription : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityExtendedDescription"/> class.
    /// </summary>
    public EntityExtendedDescription(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> valueOption)
        : base("extendeddescription", "Manages the extended description of the Publisher.")
    {
        AddCommand(new GetEntityExtendedDescriptionCommand(config, repoOption, idOption));
        AddCommand(new SetEntityExtendedDescriptionCommand(config, repoOption, idOption, valueOption));
    }
}