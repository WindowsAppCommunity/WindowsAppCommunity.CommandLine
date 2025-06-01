using System.CommandLine;
using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Publisher.ExtendedDescription;

/// <summary>
/// Wacsdk set extended description command for Publisher.
/// </summary>
public class SetEntityExtendedDescriptionCommand : Common.Entity.ExtendedDescription.SetEntityExtendedDescriptionCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetEntityExtendedDescriptionCommand"/> class.
    /// </summary>
    public SetEntityExtendedDescriptionCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> valueOption)
        : base(config, "Publisher", repoOption, idOption, valueOption)
    {
    }

    public override async Task<IModifiableEntity> GetModifiableEntityAsync(string repoId, string publisherId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var publisher = (IModifiablePublisher)await repositoryContainer.PublisherRepository.GetAsync(publisherId, cancellationToken);

        return publisher;
    }

    public override async Task PublishAsync(IModifiableEntity entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Publishing changes made to {entity.Id}");
        ModifiablePublisher publisher = (ModifiablePublisher)entity;
        await publisher.FlushAsync(cancellationToken);
    }
}