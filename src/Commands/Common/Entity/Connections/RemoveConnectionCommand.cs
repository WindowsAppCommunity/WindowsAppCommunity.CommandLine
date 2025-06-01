using System.CommandLine;
using OwlCore.Diagnostics;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Common.Connections;

/// <summary>
/// Wacsdk remove connection command.
/// </summary>
public abstract class RemoveConnectionCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveConnectionCommand"/> class.
    /// </summary>
    public RemoveConnectionCommand(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption, Option<string> connectionIdOption)
        : base("remove", $"Removes a connection from the {entityType}.")
    {
        AddOption(repoOption);
        AddOption(idOption);
        AddOption(connectionIdOption);

        this.SetHandler(InvokeAsync, repoOption, idOption, connectionIdOption);
        this.Config = config;
    }

    protected WacsdkCommandConfig Config { get; init; }

    public async Task InvokeAsync(string repo, string id, string connectionId)
    {
        var cancellationToken = Config.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();
        
        Logger.LogInformation($"Getting modifiable entity");
        var entity = await GetModifiableEntityAsync(repo, id, cancellationToken);
        Logger.LogInformation($"Got {nameof(entity.Id)}: {entity.Id}");

        await foreach (var connection in entity.GetConnectionsAsync(cancellationToken))
        {
            if (connection.Id == connectionId)
            {
                await entity.RemoveConnectionAsync(connection, cancellationToken);
                Logger.LogInformation($"Removed connection: {connection.Id}");

                await PublishAsync(entity, cancellationToken);
                return;
            }
        }

        Logger.LogWarning($"Connection with ID {connectionId} not found.");
    }

    public abstract Task<IModifiableConnectionsCollection> GetModifiableEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);

    public abstract Task PublishAsync(IModifiableConnectionsCollection entity, CancellationToken cancellationToken);
}
