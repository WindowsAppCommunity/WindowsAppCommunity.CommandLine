using System.CommandLine;
using OwlCore.Diagnostics;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Common.AccentColor;

/// <summary>
/// Wacsdk set accent color command.
/// </summary>
public abstract class SetAccentColorCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetAccentColorCommand"/> class.
    /// </summary>
    public SetAccentColorCommand(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption, Option<string> valueOption)
        : base("set", $"Sets the accent color for the {entityType}.")
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

        Logger.LogInformation($"Updating {nameof(entity.AccentColor)}: {value}");
        await entity.UpdateAccentColorAsync(value, cancellationToken);

        await PublishAsync(entity, cancellationToken);
    }

    public abstract Task<IModifiableAccentColor> GetModifiableEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);
    
    public abstract Task PublishAsync(IModifiableAccentColor entity, CancellationToken cancellationToken);
}