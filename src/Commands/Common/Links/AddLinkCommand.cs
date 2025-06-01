using System.CommandLine;
using CommunityToolkit.Diagnostics;
using OwlCore.Diagnostics;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Common.Links;

/// <summary>
/// Wacsdk add link command.
/// </summary>
public abstract class AddLinkCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddLinkCommand"/> class.
    /// </summary>
    public AddLinkCommand(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption, Option<string> linkIdOption, Option<string> nameOption, Option<string> urlOption, Option<string> descriptionOption)
        : base("add", $"Adds a link to the {entityType}.")
    {
        AddOption(repoOption);
        AddOption(idOption);
        AddOption(linkIdOption);
        AddOption(nameOption);
        AddOption(urlOption);
        AddOption(descriptionOption);

        this.SetHandler(InvokeAsync, repoOption, idOption, linkIdOption, nameOption, urlOption, descriptionOption);
        this.Config = config;
    }

    protected WacsdkCommandConfig Config { get; init; }

    public async Task InvokeAsync(string repo, string id, string linkId, string name, string url, string description)
    {
        Guard.IsNotNullOrWhiteSpace(linkId);
        Guard.IsNotNullOrWhiteSpace(name);
        Guard.IsNotNullOrWhiteSpace(url);
        
        var cancellationToken = Config.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Getting modifiable entity");
        var entity = await GetModifiableEntityAsync(repo, id, cancellationToken);
        Logger.LogInformation($"Got {nameof(entity.Id)}: {entity.Id}");

        var link = new Link()
        {
            Url = url,
            Name = name,
            Id = linkId,
            Description = description
        };

        Logger.LogInformation($"Adding link");
        Logger.LogInformation($"{nameof(link.Id)}: {link.Id}");
        Logger.LogInformation($"{nameof(link.Name)}: {link.Name}");
        Logger.LogInformation($"{nameof(link.Url)}: {link.Url}");
        Logger.LogInformation($"{nameof(link.Description)}: {link.Description}");
        await entity.AddLinkAsync(link, cancellationToken);

        await PublishAsync(entity, cancellationToken);
    }

    public abstract Task<IModifiableLinksCollection> GetModifiableEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);

    public abstract Task PublishAsync(IModifiableLinksCollection entity, CancellationToken cancellationToken);
}