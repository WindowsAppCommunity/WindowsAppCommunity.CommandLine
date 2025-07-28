using OwlCore.Storage;
using System.CommandLine;
using WindowsAppCommunity.CommandLine.Common.Profile;
using WindowsAppCommunity.CommandLine.Settings.Profile;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Commands.User.Profile;

/// <summary>
/// Update user profile page command.
/// </summary>
public class UpdateProfilePageCommand : UpdateProfilePageCommand<IReadOnlyUser>
{
    /// <inheritdoc/>
    public UpdateProfilePageCommand(WacsdkCommandConfig config, IProfileTemplateProvider<IReadOnlyUser> template, Option<string> repoOption, Option<string> idOption)
        : base(config, template, "User", repoOption, idOption)
    {
    }

    /// <inheritdoc/>
    public override async Task<IReadOnlyUser> GetEntityAsync(string repoId, string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var user = await repositoryContainer.UserRepository.GetAsync(userId, cancellationToken);

        return user;
    }
}
