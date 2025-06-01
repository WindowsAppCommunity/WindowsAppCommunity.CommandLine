using System.CommandLine;
using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Publisher.ParentPublishers;

/// <summary>
/// Wacsdk get parent publishers command for Publisher.
/// </summary>
public class GetParentPublishersCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetParentPublishersCommand"/> class.
    /// </summary>
    public GetParentPublishersCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption)
        : base("get", "Gets the parent publishers of the Publisher.")
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

        Logger.LogInformation($"Getting parent publishers");
        await foreach (var parentPublisher in publisher.ParentPublishers.GetPublishersAsync(cancellationToken))
        {
            Logger.LogInformation($"Parent Publisher ID: {parentPublisher.Id}");
            Logger.LogInformation($"Parent Publisher Name: {parentPublisher.Name}");
            Logger.LogInformation($"Parent Publisher Description: {parentPublisher.Description}");
            Logger.LogInformation("---");
        }

        Logger.LogInformation($"Parent publishers retrieved successfully");
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