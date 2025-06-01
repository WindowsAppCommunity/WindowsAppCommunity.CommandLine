using System.CommandLine;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;
using OwlCore.Diagnostics;

namespace WindowsAppCommunity.CommandLine.User.Images;

/// <summary>
/// Wacsdk remove image command for User.
/// </summary>
public class RemoveImageCommand : Common.Images.RemoveImageCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveImageCommand"/> class.
    /// </summary>
    public RemoveImageCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> imageIdOption)
        : base(config, "User", repoOption, idOption, imageIdOption)
    {
    }

    public override async Task<IModifiableImagesCollection> GetModifiableEntityAsync(string repoId, string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var user = (IModifiableUser)await repositoryContainer.UserRepository.GetAsync(userId, cancellationToken);

        return user;
    }

    public override async Task PublishAsync(IModifiableImagesCollection entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Publishing changes made to {entity.Id}");
        ModifiableUser user = (ModifiableUser)entity;
        await user.FlushAsync(cancellationToken);
    }
}