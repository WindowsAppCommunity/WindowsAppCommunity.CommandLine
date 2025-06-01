using System.CommandLine;
using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Publisher.Connections;

/// <summary>
/// Wacsdk add connection command for Publisher.
/// </summary>
public class AddConnectionCommand : Common.Connections.AddConnectionCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddConnectionCommand"/> class.
    /// </summary>
    public AddConnectionCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> connectionIdOption, Option<string> connectionValueOption)
        : base(config, "Publisher", repoOption, idOption, connectionIdOption, connectionValueOption)
    {
    }

    public override async Task<IModifiableConnectionsCollection> GetModifiableEntityAsync(string repoId, string publisherId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var publisher = (IModifiablePublisher)await repositoryContainer.PublisherRepository.GetAsync(publisherId, cancellationToken);

        return publisher;
    }

    public override async Task PublishAsync(IModifiableConnectionsCollection entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Publishing changes made to {entity.Id}");
        ModifiablePublisher publisher = (ModifiablePublisher)entity;
        await publisher.FlushAsync(cancellationToken);
    }
}