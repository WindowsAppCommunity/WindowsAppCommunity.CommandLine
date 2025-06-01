using System.CommandLine;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;
using WindowsAppCommunity.CommandLine.Common.Name;

namespace WindowsAppCommunity.CommandLine.User.Name;

/// <summary>
/// Wacsdk get name command for User.
/// </summary>
public class GetEntityNameCommand : GetNameCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetEntityNameCommand"/> class.
    /// </summary>
    public GetEntityNameCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption)
        : base(config, "User", repoOption, idOption)
    {
    }

    public override async Task<IReadOnlyEntity> GetEntityAsync(string repoId, string userId, CancellationToken cancellationToken)
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