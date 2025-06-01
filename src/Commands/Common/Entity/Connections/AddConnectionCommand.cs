using System.CommandLine;
using OwlCore.Diagnostics;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Common.Connections;

/// <summary>
/// Wacsdk add connection command.
/// </summary>
public abstract class AddConnectionCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddConnectionCommand"/> class.
    /// </summary>
    public AddConnectionCommand(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption, Option<string> connectionIdOption, Option<string> connectionValueOption)
        : base("add", $"Adds a connection to the {entityType}.")
    {
        AddOption(repoOption);
        AddOption(idOption);
        AddOption(connectionIdOption);
        AddOption(connectionValueOption);

        this.SetHandler(InvokeAsync, repoOption, idOption, connectionIdOption, connectionValueOption);
        this.Config = config;
    }

    protected WacsdkCommandConfig Config { get; init; }

    public async Task InvokeAsync(string repo, string id, string connectionId, string connectionValue)
    {
        var cancellationToken = Config.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Getting modifiable entity");
        var entity = await GetModifiableEntityAsync(repo, id, cancellationToken);
        Logger.LogInformation($"Got {nameof(entity.Id)}: {entity.Id}");

        var connection = new InMemoryConnection { Id = connectionId, Value = connectionValue };
        Logger.LogInformation($"Adding connection");
        Logger.LogInformation($"{nameof(connection.Id)}: {connection.Id}");
        Logger.LogInformation($"{nameof(connection.Value)}: {connection.Value}");
        await entity.AddConnectionAsync(connection, cancellationToken);

        await PublishAsync(entity, cancellationToken);
    }

    public abstract Task<IModifiableConnectionsCollection> GetModifiableEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);

    public abstract Task PublishAsync(IModifiableConnectionsCollection entity, CancellationToken cancellationToken);
}
