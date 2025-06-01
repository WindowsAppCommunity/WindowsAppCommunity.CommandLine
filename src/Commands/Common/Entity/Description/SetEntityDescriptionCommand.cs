using System.CommandLine;
using OwlCore.Diagnostics;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Common.Entity.Description;

/// <summary>
/// Wacsdk set entity description command.
/// </summary>
public abstract class SetEntityDescriptionCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetEntityDescriptionCommand"/> class.
    /// </summary>
    public SetEntityDescriptionCommand(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption, Option<string> valueOption)
        : base("set", $"Sets the description for the {entityType}.")
    {
        AddOption(repoOption);
        AddOption(idOption);
        AddOption(valueOption);

        this.SetHandler(InvokeAsync, repoOption, idOption, valueOption);
        this.Config = config;
    }

    protected WacsdkCommandConfig Config { get; init; }

    public async Task InvokeAsync(string repo, string id, string value)
    {
        var cancellationToken = Config.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Getting modifiable entity");
        var entity = await GetModifiableEntityAsync(repo, id, cancellationToken);
        Logger.LogInformation($"Got {nameof(entity.Id)}: {entity.Id}");

        Logger.LogInformation($"Updating {nameof(entity.Description)}: {value}");
        await entity.UpdateDescriptionAsync(value, cancellationToken);

        await PublishAsync(entity, cancellationToken);
    }

    public abstract Task<IModifiableEntity> GetModifiableEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);
    
    public abstract Task PublishAsync(IModifiableEntity entity, CancellationToken cancellationToken);
}