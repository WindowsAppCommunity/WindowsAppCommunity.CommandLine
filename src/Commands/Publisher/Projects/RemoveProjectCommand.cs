using System.CommandLine;
using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Publisher.Projects;

/// <summary>
/// Wacsdk remove project command for Publisher.
/// </summary>
public class RemoveProjectCommand : Common.Projects.RemoveProjectCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveProjectCommand"/> class.
    /// </summary>
    public RemoveProjectCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> projectIdOption)
        : base(config, "Publisher", repoOption, idOption, projectIdOption)
    {
    }

    public override async Task<IModifiableProjectCollection<IReadOnlyProject>> GetModifiableEntityAsync(string repoId, string publisherId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var publisher = (IModifiablePublisher)await repositoryContainer.PublisherRepository.GetAsync(publisherId, cancellationToken);

        return publisher;
    }

    public override async Task PublishAsync(IModifiableProjectCollection<IReadOnlyProject> entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Publishing changes made to {entity.Id}");
        ModifiablePublisher publisher = (ModifiablePublisher)entity;
        await publisher.FlushAsync(cancellationToken);
    }
}