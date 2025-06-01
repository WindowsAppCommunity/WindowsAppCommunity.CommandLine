using System.CommandLine;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Publisher.UserRoles;

/// <summary>
/// Wacsdk get user roles command for Publisher.
/// </summary>
public class GetUserRolesCommands : Common.UserRoles.GetUserRolesCommands
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserRolesCommands"/> class.
    /// </summary>
    public GetUserRolesCommands(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption)
        : base(config, "Publisher", repoOption, idOption)
    {
    }

    public override async Task<IReadOnlyUserRoleCollection> GetEntityAsync(string repoId, string publisherId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var publisher = await repositoryContainer.PublisherRepository.GetAsync(publisherId, cancellationToken);

        return publisher;
    }
}