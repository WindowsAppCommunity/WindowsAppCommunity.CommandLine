using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.Project.ForgetMe;

/// <summary>
/// Wacsdk entity forget me command for Project.
/// </summary>
public class EntityForgetMeCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityForgetMeCommand"/> class.
    /// </summary>
    public EntityForgetMeCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<bool?> valueOption)
        : base("forget-me", "Manages the forget me flag of the Project.")
    {
        AddCommand(new GetEntityForgetMeCommand(config, repoOption, idOption));
        AddCommand(new SetEntityForgetMeCommand(config, repoOption, idOption, valueOption));
    }
}