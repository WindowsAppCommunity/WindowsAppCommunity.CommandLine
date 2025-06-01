using System.CommandLine;
using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Publisher.Links;

/// <summary>
/// Wacsdk remove link command for Publisher.
/// </summary>
public class RemoveLinkCommand : Common.Links.RemoveLinkCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveLinkCommand"/> class.
    /// </summary>
    public RemoveLinkCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> linkNameOption)
        : base(config, "Publisher", repoOption, idOption, linkNameOption)
    {
    }

    public override async Task<IModifiableLinksCollection> GetModifiableEntityAsync(string repoId, string publisherId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var publisher = (IModifiablePublisher)await repositoryContainer.PublisherRepository.GetAsync(publisherId, cancellationToken);

        return publisher;
    }

    public override async Task PublishAsync(IModifiableLinksCollection entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Publishing changes made to {entity.Id}");
        ModifiablePublisher publisher = (ModifiablePublisher)entity;
        await publisher.FlushAsync(cancellationToken);
    }
}