using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.User.ForgetMe;

/// <summary>
/// Wacsdk entity forget me command for User.
/// </summary>
public class EntityForgetMeCommand : Command
{    /// <summary>
    /// Initializes a new instance of the <see cref="EntityForgetMeCommand"/> class.
    /// </summary>
    public EntityForgetMeCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<bool?> forgetMeOption)
        : base("forget-me", "Manages the forget me setting of the User.")
    {
        AddCommand(new GetEntityForgetMeCommand(config, repoOption, idOption));
        AddCommand(new SetEntityForgetMeCommand(config, repoOption, idOption, forgetMeOption));
    }
}