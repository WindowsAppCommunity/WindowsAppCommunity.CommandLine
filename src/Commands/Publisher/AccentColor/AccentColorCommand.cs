using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.Publisher.AccentColor;

/// <summary>
/// Wacsdk accent color command for Publisher.
/// </summary>
public class AccentColorCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AccentColorCommand"/> class.
    /// </summary>
    public AccentColorCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> valueOption)
        : base("accent-color", "Manages the accent color of the Publisher.")
    {
        AddCommand(new GetAccentColorCommand(config, repoOption, idOption));
        AddCommand(new SetAccentColorCommand(config, repoOption, idOption, valueOption));
    }
}