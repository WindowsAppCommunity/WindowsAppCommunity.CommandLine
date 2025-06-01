using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.Project.ExtendedDescription;

/// <summary>
/// Wacsdk entity extended description command for Project.
/// </summary>
public class EntityExtendedDescription : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityExtendedDescription"/> class.
    /// </summary>
    public EntityExtendedDescription(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> valueOption)
        : base("extended-description", "Manages the extended description of the Project.")
    {
        AddCommand(new GetEntityExtendedDescriptionCommand(config, repoOption, idOption));
        AddCommand(new SetEntityExtendedDescriptionCommand(config, repoOption, idOption, valueOption));
    }
}