using System.CommandLine;
using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;
using WindowsAppCommunity.CommandLine.Common.Name;

namespace WindowsAppCommunity.CommandLine.Project.Name;

/// <summary>
/// Wacsdk set name command for Project.
/// </summary>
public class SetEntityNameCommand : SetNameCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetEntityNameCommand"/> class.
    /// </summary>
    public SetEntityNameCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> valueOption)
        : base(config, "Project", repoOption, idOption, valueOption)
    {
    }

    public override async Task<IModifiableEntity> GetModifiableEntityAsync(string repoId, string projectId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var project = (IModifiableProject)await repositoryContainer.ProjectRepository.GetAsync(projectId, cancellationToken);

        return project;
    }

    public override async Task PublishAsync(IModifiableEntity entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Publishing changes made to {entity.Id}");
        ModifiableProject project = (ModifiableProject)entity;
        await project.FlushAsync(cancellationToken);
    }
}