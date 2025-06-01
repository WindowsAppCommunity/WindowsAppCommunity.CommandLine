using System.CommandLine;
using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Publisher.UserRoles;

/// <summary>
/// Wacsdk remove user role command for Publisher.
/// </summary>
public class RemoveUserRoleCommand : Common.UserRoles.RemoveUserRoleCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveUserRoleCommand"/> class.
    /// </summary>
    public RemoveUserRoleCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> userIdOption)
        : base(config, "Publisher", repoOption, idOption, userIdOption)
    {
    }

    public override async Task<IModifiableUserRoleCollection> GetModifiableEntityAsync(string repoId, string publisherId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var publisher = (IModifiablePublisher)await repositoryContainer.PublisherRepository.GetAsync(publisherId, cancellationToken);

        return publisher;
    }

    public override async Task<IReadOnlyUser> GetUserByIdAsync(string repoId, string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var user = await repositoryContainer.UserRepository.GetAsync(userId, cancellationToken);

        return user;
    }

    public override async Task PublishAsync(IModifiableUserRoleCollection entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Publishing changes made to {entity.Id}");
        ModifiablePublisher publisher = (ModifiablePublisher)entity;
        await publisher.FlushAsync(cancellationToken);
    }
}