using System.CommandLine;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;
using OwlCore.Diagnostics;

namespace WindowsAppCommunity.CommandLine.User.PublisherRoles;

/// <summary>
/// Wacsdk add publisher role command for User.
/// </summary>
public class AddPublisherRoleCommand : Common.PublisherRoles.AddPublisherRoleCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddPublisherRoleCommand"/> class.
    /// </summary>
    public AddPublisherRoleCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> publisherIdOption, Option<string> roleIdOption, Option<string> roleNameOption, Option<string> roleDescriptionOption)
        : base(config, "User", repoOption, idOption, publisherIdOption, roleIdOption, roleNameOption, roleDescriptionOption)
    {
    }

    public override async Task<IModifiablePublisherRoleCollection> GetModifiableEntityAsync(string repoId, string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var user = (IModifiableUser)await repositoryContainer.UserRepository.GetAsync(userId, cancellationToken);

        return user;
    }

    public override async Task<IReadOnlyPublisher> GetPublisherByIdAsync(string repoId, string publisherId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var publisher = await repositoryContainer.PublisherRepository.GetAsync(publisherId, cancellationToken);

        return publisher;
    }

    public override async Task PublishAsync(IModifiablePublisherRoleCollection entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Publishing changes made to {entity.Id}");
        ModifiableUser user = (ModifiableUser)entity;
        await user.FlushAsync(cancellationToken);
    }
}