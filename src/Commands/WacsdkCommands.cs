using System.CommandLine;
using WindowsAppCommunity.CommandLine.Repo;
using WindowsAppCommunity.CommandLine.Publisher;
using WindowsAppCommunity.CommandLine.User;
using WindowsAppCommunity.CommandLine.Project;

namespace WindowsAppCommunity.CommandLine;

/// <summary>
/// Commands for interacting with locally running Kubo nodes.
/// </summary>
public class WacsdkCommand : Command
{
    /// <summary>
    /// Creates a new instance of <see cref="WacsdkCommand"/>.
    /// </summary>
    public WacsdkCommand(WacsdkCommandConfig config) : base("wacsdk", "Commands for interacting with the Windows App Community SDK.")
    {
        AddCommand(new WacsdkRepoCommands(config));

        var repoOption = new Option<string>(
            name: "--repo-id",
            () => "default",
            description: "The ID of the WACSDK repository.")
        {
            IsRequired = true,
        };

        AddCommand(new WacsdkUserCommands(config, repoOption));
        AddCommand(new WacsdkProjectCommands(config, repoOption));
        AddCommand(new WacsdkPublisherCommands(config, repoOption));
    }
}
