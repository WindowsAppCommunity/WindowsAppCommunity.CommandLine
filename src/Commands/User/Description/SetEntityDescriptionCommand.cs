using System.CommandLine;
using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.User.Description;

/// <summary>
/// Wacsdk set description command for User.
/// </summary>
public class SetEntityDescriptionCommand : Common.Entity.Description.SetEntityDescriptionCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetEntityDescriptionCommand"/> class.
    /// </summary>
    public SetEntityDescriptionCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> valueOption)
        : base(config, "User", repoOption, idOption, valueOption)
    {
    }

    public override async Task<IModifiableEntity> GetModifiableEntityAsync(string repoId, string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var user = (IModifiableUser)await repositoryContainer.UserRepository.GetAsync(userId, cancellationToken);

        return user;
    }

    public override async Task PublishAsync(IModifiableEntity entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Publishing changes made to {entity.Id}");
        ModifiableUser user = (ModifiableUser)entity;
        await user.FlushAsync(cancellationToken);
    }
}