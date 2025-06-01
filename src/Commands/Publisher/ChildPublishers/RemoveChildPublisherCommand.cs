using System.CommandLine;
using CommunityToolkit.Diagnostics;
using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Publisher.ChildPublishers;

/// <summary>
/// Wacsdk remove child publisher command for Publisher.
/// </summary>
public class RemoveChildPublisherCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveChildPublisherCommand"/> class.
    /// </summary>
    public RemoveChildPublisherCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> publisherIdOption)
        : base("remove", "Removes a child publisher from the Publisher.")
    {
        AddOption(repoOption);
        AddOption(idOption);
        AddOption(publisherIdOption);

        this.SetHandler(InvokeAsync, repoOption, idOption, publisherIdOption);
        this.Config = config;
    }

    protected WacsdkCommandConfig Config { get; init; }

    public async Task InvokeAsync(string repo, string id, string publisherId)
    {
        Guard.IsNotNullOrWhiteSpace(publisherId);

        var cancellationToken = Config.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Getting modifiable publisher");
        var publisher = (ModifiablePublisher)await GetPublisherByIdAsync(repo, id, cancellationToken);
        Logger.LogInformation($"Got {nameof(publisher.Id)}: {publisher.Id}");

        Logger.LogInformation($"Getting child publisher to remove");
        var childPublisher = await GetPublisherByIdAsync(repo, publisherId, cancellationToken);
        Logger.LogInformation($"Got {nameof(childPublisher.Id)}: {childPublisher.Id}");

        Logger.LogInformation($"Removing child publisher from collection");
        await publisher.ChildPublishers.RemovePublisherAsync(childPublisher, cancellationToken);

        Logger.LogInformation($"Publishing changes");
        await PublishAsync(publisher, cancellationToken);

        Logger.LogInformation($"Child publisher removed successfully");
    }

    public async Task<IReadOnlyPublisher> GetPublisherByIdAsync(string repoId, string publisherId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var publisher = await repositoryContainer.PublisherRepository.GetAsync(publisherId, cancellationToken);

        return publisher;
    }

    public async Task PublishAsync(IModifiablePublisher publisher, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Publishing changes made to {publisher.Id}");
        ModifiablePublisher modifiablePublisher = (ModifiablePublisher)publisher;
        await modifiablePublisher.FlushAsync(cancellationToken);
    }
}