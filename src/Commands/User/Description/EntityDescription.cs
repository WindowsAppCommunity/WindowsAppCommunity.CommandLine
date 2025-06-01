using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.User.Description;

/// <summary>
/// Wacsdk entity description command for User.
/// </summary>
public class EntityDescription : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityDescription"/> class.
    /// </summary>
    public EntityDescription(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> valueOption)
        : base("description", "Manages the description of the User.")
    {
        AddCommand(new GetEntityDescriptionCommand(config, repoOption, idOption));
        AddCommand(new SetEntityDescriptionCommand(config, repoOption, idOption, valueOption));
    }
}