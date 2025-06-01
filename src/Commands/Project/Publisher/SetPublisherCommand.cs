using System.CommandLine;
using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Project.Publisher;

/// <summary>
/// Wacsdk set publisher command.
/// </summary>
public class SetPublisherCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetPublisherCommand"/> class.
    /// </summary>
    public SetPublisherCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> publisherIdOption)
        : base("set", "Sets the publisher for the project.")
    {
        AddOption(repoOption);
        AddOption(idOption);
        AddOption(publisherIdOption);

        this.SetHandler(InvokeAsync, repoOption, idOption, publisherIdOption);
        this.Config = config;
    }

    protected WacsdkCommandConfig Config { get; set; }

    public async Task InvokeAsync(string repo, string id, string publisherId)
    {
        var cancellationToken = Config.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Getting modifiable project");
        var project = await GetModifiableProjectAsync(repo, id, Config.CancellationToken);
        Logger.LogInformation($"Got {nameof(project.Id)}: {project.Id}");
        
        // Get the publisher from the repository
        var repositoryContainer = await GetRepositoryContainerAsync(repo, Config.CancellationToken);
        var publisher = await repositoryContainer.PublisherRepository.GetAsync(publisherId, Config.CancellationToken);

        Logger.LogInformation($"Updating publisher: {publisherId}");
        await project.UpdatePublisherAsync(publisher, Config.CancellationToken);

        await PublishAsync(project, Config.CancellationToken);
    }

    public async Task<IModifiableProject> GetModifiableProjectAsync(string repoId, string projectId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var project = (IModifiableProject)await repositoryContainer.ProjectRepository.GetAsync(projectId, cancellationToken);

        return project;
    }

    public async Task<RepositoryContainer> GetRepositoryContainerAsync(string repoId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        return new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
    }

    public async Task PublishAsync(IModifiableProject entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Logger.LogInformation($"Publishing changes made to {entity.Id}");
        ModifiableProject project = (ModifiableProject)entity;
        await project.FlushAsync(cancellationToken);
    }
}