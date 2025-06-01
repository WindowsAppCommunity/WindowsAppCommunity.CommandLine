using System.CommandLine;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Project.IsUnlisted;

/// <summary>
/// Wacsdk get entity is unlisted command for Project.
/// </summary>
public class GetEntityIsUnlistedCommand : Common.Entity.IsUnlisted.GetEntityIsUnlistedCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetEntityIsUnlistedCommand"/> class.
    /// </summary>
    public GetEntityIsUnlistedCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption)
        : base(config, "Project", repoOption, idOption)
    {
    }

    public override async Task<IReadOnlyEntity> GetEntityAsync(string repoId, string projectId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var project = await repositoryContainer.ProjectRepository.GetAsync(projectId, cancellationToken);

        return project;
    }
}