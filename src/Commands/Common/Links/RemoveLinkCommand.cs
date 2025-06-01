using System.CommandLine;
using OwlCore.Diagnostics;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Common.Links;

/// <summary>
/// Wacsdk remove link command.
/// </summary>
public abstract class RemoveLinkCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveLinkCommand"/> class.
    /// </summary>
    public RemoveLinkCommand(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption, Option<string> linkIdOption)
        : base("remove", $"Removes a link from the {entityType}.")
    {
        AddOption(repoOption);
        AddOption(idOption);
        AddOption(linkIdOption);

        this.SetHandler(InvokeAsync, repoOption, idOption, linkIdOption);
        this.Config = config;
    }

    protected WacsdkCommandConfig Config { get; init; }

    public async Task InvokeAsync(string repo, string id, string linkId)
    {
        var cancellationToken = Config.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Getting modifiable entity");
        var entity = await GetModifiableEntityAsync(repo, id, cancellationToken);
        Logger.LogInformation($"Got {nameof(entity.Id)}: {entity.Id}");

        var linkToRemove = entity.Links.FirstOrDefault(link => $"{link.Id}" == $"{linkId}");
        if (linkToRemove != null)
        {
            await entity.RemoveLinkAsync(linkToRemove, cancellationToken);

            Logger.LogInformation($"Removed link");
            Logger.LogInformation($"{nameof(linkToRemove.Id)}: {linkToRemove.Id}");
            Logger.LogInformation($"{nameof(linkToRemove.Name)}: {linkToRemove.Name}");
            Logger.LogInformation($"{nameof(linkToRemove.Url)}: {linkToRemove.Url}");
            Logger.LogInformation($"{nameof(linkToRemove.Description)}: {linkToRemove.Description}");

            await PublishAsync(entity, cancellationToken);
        }
        else
        {
            Logger.LogWarning($"Link with ID {linkId} not found.");
        }
    }

    public abstract Task<IModifiableLinksCollection> GetModifiableEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);
    
    public abstract Task PublishAsync(IModifiableLinksCollection entity, CancellationToken cancellationToken);
}