using System.CommandLine;
using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Project.Links;

/// <summary>
/// Wacsdk add link command for Project.
/// </summary>
public class AddLinkCommand : Common.Links.AddLinkCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddLinkCommand"/> class.
    /// </summary>
    public AddLinkCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> linkIdOption, Option<string> nameOption, Option<string> urlOption, Option<string> descriptionOption)
        : base(config, "Project", repoOption, idOption, linkIdOption, nameOption, urlOption, descriptionOption)
    {
    }

    public override async Task<IModifiableLinksCollection> GetModifiableEntityAsync(string repoId, string projectId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var project = (IModifiableProject)await repositoryContainer.ProjectRepository.GetAsync(projectId, cancellationToken);

        return project;
    }

    public override async Task PublishAsync(IModifiableLinksCollection entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Publishing changes made to {entity.Id}");
        ModifiableProject project = (ModifiableProject)entity;
        await project.FlushAsync(cancellationToken);
    }
}