using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.Project.Features;

/// <summary>
/// Wacsdk features command.
/// </summary>
public class FeaturesCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FeaturesCommand"/> class.
    /// </summary>
    public FeaturesCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> projectIdOption, Option<string> featureOption)
        : base("features", "Features get, add and remove commands.")
    {
        AddCommand(new GetFeaturesCommand(config, repoOption, projectIdOption));
        AddCommand(new AddFeatureCommand(config, repoOption, projectIdOption, featureOption));
        AddCommand(new RemoveFeatureCommand(config, repoOption, projectIdOption, featureOption));
    }
}