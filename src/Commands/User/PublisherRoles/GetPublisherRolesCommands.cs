using System.CommandLine;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.User.PublisherRoles;

/// <summary>
/// Wacsdk get publisher roles command for User.
/// </summary>
public class GetPublisherRolesCommands : Common.PublisherRoles.GetPublisherRolesCommands
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPublisherRolesCommands"/> class.
    /// </summary>
    public GetPublisherRolesCommands(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption)
        : base(config, "User", repoOption, idOption)
    {
    }

    public override async Task<IReadOnlyPublisherRoleCollection> GetEntityAsync(string repoId, string userId, CancellationToken cancellationToken)
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