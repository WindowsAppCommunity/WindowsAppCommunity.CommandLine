using System.CommandLine;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;
using OwlCore.Diagnostics;

namespace WindowsAppCommunity.CommandLine.User.ProjectRoles;

/// <summary>
/// Wacsdk remove project role command for User.
/// </summary>
public class RemoveProjectRoleCommand : Common.ProjectRoles.RemoveProjectRoleCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveProjectRoleCommand"/> class.
    /// </summary>
    public RemoveProjectRoleCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> projectRoleIdOption)
        : base(config, "User", repoOption, idOption, projectRoleIdOption)
    {
    }

    public override async Task<IModifiableProjectRoleCollection> GetModifiableEntityAsync(string repoId, string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var user = (IModifiableUser)await repositoryContainer.UserRepository.GetAsync(userId, cancellationToken);

        return user;
    }

    public override async Task<IReadOnlyProject> GetProjectByIdAsync(string repoId, string projectId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var project = await repositoryContainer.ProjectRepository.GetAsync(projectId, cancellationToken);

        return project;
    }

    public override async Task PublishAsync(IModifiableProjectRoleCollection entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Publishing changes made to {entity.Id}");
        ModifiableUser user = (ModifiableUser)entity;
        await user.FlushAsync(cancellationToken);
    }
}