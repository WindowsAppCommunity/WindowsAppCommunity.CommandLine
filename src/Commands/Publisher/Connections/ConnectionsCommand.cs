using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.Publisher.Connections;

/// <summary>
/// Wacsdk connections command for Publisher.
/// </summary>
public class ConnectionsCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionsCommand"/> class.
    /// </summary>
    public ConnectionsCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> connectionIdOption, Option<string> connectionValueOption)
        : base("connections", "Manages the connections of the Publisher.")
    {
        AddCommand(new GetConnectionsCommand(config, repoOption, idOption));
        AddCommand(new AddConnectionCommand(config, repoOption, idOption, connectionIdOption, connectionValueOption));
        AddCommand(new RemoveConnectionCommand(config, repoOption, idOption, connectionIdOption));
    }
}