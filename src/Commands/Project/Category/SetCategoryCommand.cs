using System.CommandLine;
using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Project.Category;

/// <summary>
/// Wacsdk set category command.
/// </summary>
public class SetCategoryCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetCategoryCommand"/> class.
    /// </summary>
    public SetCategoryCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> categoryOption)
        : base("set", "Sets the category for the project.")
    {
        AddOption(repoOption);
        AddOption(idOption);
        AddOption(categoryOption);

        this.SetHandler(InvokeAsync, repoOption, idOption, categoryOption);
        this.Config = config;
    }

    protected WacsdkCommandConfig Config { get; set; }
    
    public async Task InvokeAsync(string repo, string id, string category)
    {
        var cancellationToken = Config.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Getting modifiable project");
        var project = await GetModifiableProjectAsync(repo, id, cancellationToken);
        Logger.LogInformation($"Got {nameof(project.Id)}: {project.Id}");

        Logger.LogInformation($"Updating {nameof(IReadOnlyProject.Category)}: {category}");
        await project.UpdateCategoryAsync(category, cancellationToken);

        await PublishAsync(project, cancellationToken);
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

    public async Task PublishAsync(IModifiableProject project, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Publishing changes made to {project.Id}");
        WindowsAppCommunity.Sdk.Nomad.ModifiableProject concreteProject = (WindowsAppCommunity.Sdk.Nomad.ModifiableProject)project;
        await concreteProject.FlushAsync(cancellationToken);
    }
}