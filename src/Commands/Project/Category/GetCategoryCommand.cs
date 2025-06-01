using System.CommandLine;
using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Project.Category;

/// <summary>
/// Wacsdk get category command.
/// </summary>
public class GetCategoryCommand : Command
{    /// <summary>
    /// Initializes a new instance of the <see cref="GetCategoryCommand"/> class.
    /// </summary>
    public GetCategoryCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption)
        : base("get", "Gets the category for the project.")
    {
        AddOption(repoOption);
        AddOption(idOption);

        this.SetHandler(InvokeAsync, repoOption, idOption);
        this.Config = config;
    }

    protected WacsdkCommandConfig Config { get; set; }

    public async Task InvokeAsync(string repo, string id)
    {
        var project = await GetProjectAsync(repo, id, Config.CancellationToken);
        Logger.LogInformation($"{nameof(project.Id)}: {project.Id}");
        Logger.LogInformation($"{nameof(project.Category)}: {project.Category}");
    }

    public async Task<IReadOnlyProject> GetProjectAsync(string repoId, string projectId, CancellationToken cancellationToken)
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