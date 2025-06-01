using System.CommandLine;
using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Project.Features;

/// <summary>
/// Wacsdk add feature command.
/// </summary>
public class AddFeatureCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddFeatureCommand"/> class.
    /// </summary>
    public AddFeatureCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> featureOption)
        : base("add", "Adds a feature to the project.")
    {
        AddOption(repoOption);
        AddOption(idOption);
        AddOption(featureOption);

        this.SetHandler(InvokeAsync, repoOption, idOption, featureOption);
        this.Config = config;
    }

    protected WacsdkCommandConfig Config { get; set; }

    public async Task InvokeAsync(string repo, string id, string feature)
    {
        var cancellationToken = Config.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Getting modifiable project");
        var project = await GetModifiableProjectAsync(repo, id, Config.CancellationToken);
        Logger.LogInformation($"Got {nameof(project.Id)}: {project.Id}");

        if (!project.Features.Contains(feature))
        {
            Logger.LogInformation($"Adding feature: {feature}");
            await project.AddFeatureAsync(feature, Config.CancellationToken);
            await PublishAsync(project, Config.CancellationToken);
        }
        else
        {
            Logger.LogInformation($"Feature '{feature}' already exists in project {project.Id}");
        }
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
        
        ModifiableProject modifiableProject = (ModifiableProject)project;
        await modifiableProject.FlushAsync(cancellationToken);
    }
}