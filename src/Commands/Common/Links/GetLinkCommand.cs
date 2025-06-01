using System.CommandLine;
using OwlCore.Diagnostics;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Common.Links;

/// <summary>
/// Wacsdk get links command.
/// </summary>
public abstract class GetLinkCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetLinkCommand"/> class.
    /// </summary>
    public GetLinkCommand(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption)
        : base("get", $"Gets the links for the {entityType}.")
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

        Logger.LogInformation($"Getting modifiable entity");
        var entity = await GetEntityAsync(repo, id, cancellationToken);
        Logger.LogInformation($"Got {nameof(entity.Id)}: {entity.Id}");

        Logger.LogInformation($"Enumerating links");
        foreach (var link in entity.Links)
        {
            Logger.LogInformation($"Link");
            Logger.LogInformation($"    {nameof(link.Id)}: {link.Id}");
            Logger.LogInformation($"    {nameof(link.Name)}: {link.Name}");
            Logger.LogInformation($"    {nameof(link.Url)}: {link.Url}");
            Logger.LogInformation($"    {nameof(link.Description)}: {link.Description}");
        }
    }

    public abstract Task<IReadOnlyLinksCollection> GetEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);
}