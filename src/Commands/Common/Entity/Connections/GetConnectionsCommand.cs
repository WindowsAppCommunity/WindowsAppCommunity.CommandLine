using System.CommandLine;
using OwlCore.Diagnostics;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Common.Connections;

/// <summary>
/// Wacsdk get connections command.
/// </summary>
public abstract class GetConnectionsCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetConnectionsCommand"/> class.
    /// </summary>
    public GetConnectionsCommand(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption)
        : base("get", $"Gets the connections for the {entityType}.")
    {
        AddOption(repoOption);
        AddOption(idOption);

        this.SetHandler(InvokeAsync, repoOption, idOption);
        this.Config = config;
    }

    protected WacsdkCommandConfig Config { get; init; }

    public async Task InvokeAsync(string repo, string id)
    {
        var cancellationToken = Config.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Getting entity");
        var entity = await GetEntityAsync(repo, id, cancellationToken);
        Logger.LogInformation($"Got {nameof(entity.Id)}: {entity.Id}");
        
        Logger.LogInformation($"Enumerating connections");
        await foreach (var connection in entity.GetConnectionsAsync(cancellationToken))
        {
            Logger.LogInformation($"Connection: {connection.Id}");
        }
    }

    public abstract Task<IReadOnlyConnectionsCollection> GetEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);
}
