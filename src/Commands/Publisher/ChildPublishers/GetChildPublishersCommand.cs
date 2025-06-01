using System.CommandLine;
using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Publisher.ChildPublishers;

/// <summary>
/// Wacsdk get child publishers command for Publisher.
/// </summary>
public class GetChildPublishersCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetChildPublishersCommand"/> class.
    /// </summary>
    public GetChildPublishersCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption)
        : base("get", "Gets the child publishers of the Publisher.")
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

        Logger.LogInformation($"Getting publisher");
        var publisher = await GetPublisherAsync(repo, id, cancellationToken);
        Logger.LogInformation($"Got {nameof(publisher.Id)}: {publisher.Id}");

        Logger.LogInformation($"Getting child publishers");
        await foreach (var childPublisher in publisher.ChildPublishers.GetPublishersAsync(cancellationToken))
        {
            Logger.LogInformation($"Child Publisher ID: {childPublisher.Id}");
            Logger.LogInformation($"Child Publisher Name: {childPublisher.Name}");
            Logger.LogInformation($"Child Publisher Description: {childPublisher.Description}");
            Logger.LogInformation("---");
        }

        Logger.LogInformation($"Child publishers retrieved successfully");
    }

    public async Task<IReadOnlyPublisher> GetPublisherAsync(string repoId, string publisherId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var publisher = await repositoryContainer.PublisherRepository.GetAsync(publisherId, cancellationToken);

        return publisher;
    }
}