using System.CommandLine;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Project.AccentColor;

/// <summary>
/// Wacsdk get accent color command for Project.
/// </summary>
public class GetAccentColorCommand : Common.AccentColor.GetAccentColorCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetAccentColorCommand"/> class.
    /// </summary>
    public GetAccentColorCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption)
        : base(config, "Project", repoOption, idOption)
    {
    }

    public override async Task<IReadOnlyAccentColor> GetEntityAsync(string repoId, string projectId, CancellationToken cancellationToken)
    {
        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var project = await repositoryContainer.ProjectRepository.GetAsync(projectId, cancellationToken);

        return project;
    }
}