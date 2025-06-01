using System.CommandLine;
using OwlCore.Diagnostics;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Common.Entity.IsUnlisted;

/// <summary>
/// Wacsdk get entity is unlisted command.
/// </summary>
public abstract class GetEntityIsUnlistedCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetEntityIsUnlistedCommand"/> class.
    /// </summary>
    public GetEntityIsUnlistedCommand(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption)
        : base("get", $"Gets the is unlisted status for the {entityType}.")
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
        Logger.LogInformation($"{nameof(entity.IsUnlisted)}: {entity.IsUnlisted}");
    }

    public abstract Task<IReadOnlyEntity> GetEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);
}