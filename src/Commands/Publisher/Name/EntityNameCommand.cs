using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.Publisher.Name;

/// <summary>
/// Wacsdk entity name command for Publisher.
/// </summary>
public class EntityNameCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNameCommand"/> class.
    /// </summary>
    public EntityNameCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> valueOption)
        : base("name", "Manages the name of the Publisher.")
    {
        AddCommand(new GetEntityNameCommand(config, repoOption, idOption));
        AddCommand(new SetEntityNameCommand(config, repoOption, idOption, valueOption));
    }
}