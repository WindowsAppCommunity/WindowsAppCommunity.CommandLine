using System.CommandLine;
using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Project.Connections;

/// <summary>
/// Wacsdk add connection command for Project.
/// </summary>
public class AddConnectionCommand : Common.Connections.AddConnectionCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddConnectionCommand"/> class.
    /// </summary>
    public AddConnectionCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> connectionIdOption, Option<string> connectionValueOption)
        : base(config, "Project", repoOption, idOption, connectionIdOption, connectionValueOption)
    {
    }

    public override async Task<IModifiableConnectionsCollection> GetModifiableEntityAsync(string repoId, string projectId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var project = (IModifiableProject)await repositoryContainer.ProjectRepository.GetAsync(projectId, cancellationToken);

        return project;
    }

    public override async Task PublishAsync(IModifiableConnectionsCollection entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Publishing changes made to {entity.Id}");
        ModifiableProject project = (ModifiableProject)entity;
        await project.FlushAsync(cancellationToken);
    }
}