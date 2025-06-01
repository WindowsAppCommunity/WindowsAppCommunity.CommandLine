using System.CommandLine;
using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Project.Images;

/// <summary>
/// Wacsdk add image command for Project.
/// </summary>
public class AddImageCommand : Common.Images.AddImageCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddImageCommand"/> class.
    /// </summary>
    public AddImageCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> imagePathOption, Option<string?> imageIdOption, Option<string?> imageNameOption)
        : base(config, "Project", repoOption, idOption, imagePathOption, imageIdOption, imageNameOption)
    {
    }

    public override async Task<IModifiableImagesCollection> GetModifiableEntityAsync(string repoId, string projectId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var project = (IModifiableProject)await repositoryContainer.ProjectRepository.GetAsync(projectId, cancellationToken);

        return project;
    }

    public override async Task PublishAsync(IModifiableImagesCollection entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Publishing changes made to {entity.Id}");
        ModifiableProject project = (ModifiableProject)entity;
        await project.FlushAsync(cancellationToken);
    }
}