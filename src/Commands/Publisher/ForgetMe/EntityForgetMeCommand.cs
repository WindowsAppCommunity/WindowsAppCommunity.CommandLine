using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.Publisher.ForgetMe;

/// <summary>
/// Wacsdk entity forget me command for Publisher.
/// </summary>
public class EntityForgetMeCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityForgetMeCommand"/> class.
    /// </summary>
    public EntityForgetMeCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<bool?> valueOption)
        : base("forgetme", "Manages the forget me status of the Publisher.")
    {
        AddCommand(new GetEntityForgetMeCommand(config, repoOption, idOption));
        AddCommand(new SetEntityForgetMeCommand(config, repoOption, idOption, valueOption));
    }
}