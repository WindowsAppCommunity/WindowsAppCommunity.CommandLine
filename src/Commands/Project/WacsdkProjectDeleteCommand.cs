using OwlCore.Diagnostics;
using OwlCore.Storage;
using System.CommandLine;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Project
{
    /// <summary>
    /// Command to delete an existing project.
    /// </summary>
    public class WacsdkProjectDeleteCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WacsdkProjectDeleteCommand"/> class.
        /// </summary>
        public WacsdkProjectDeleteCommand(WacsdkCommandConfig config, Option<string> repoOption)
            : base(name: "delete", description: "Deletes a project.")
        {
            var projectIdOption = new Option<string>(
                name: "--project-id",
                description: "The ID of the project to delete.")
            {
                IsRequired = true
            };

            AddOption(repoOption);
            AddOption(projectIdOption);
            this.SetHandler(InvokeAsync, repoOption, projectIdOption);

            Config = config;
        }

        /// <summary>
        /// Shared command configuration.
        /// </summary>
        public WacsdkCommandConfig Config { get; }

        /// <summary>
        /// Handles the command.
        /// </summary>
        public async Task InvokeAsync(string repoId, string projectId)
        {
            var thisRepoStorage = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false);

            Logger.LogInformation($"Getting repo store with ID {repoId} at {thisRepoStorage.GetType().Name} {thisRepoStorage.Id}");
            var repoSettings = new WacsdkNomadSettings(thisRepoStorage);
            await repoSettings.LoadAsync(Config.CancellationToken);

            var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);

            Logger.LogInformation($"Getting project {projectId}");
            var project = (ModifiableProject)await repositoryContainer.ProjectRepository.GetAsync(projectId, Config.CancellationToken);

            Logger.LogInformation($"Deleting project {projectId}");
            await repositoryContainer.ProjectRepository.DeleteAsync(project, Config.CancellationToken);

            Logger.LogInformation($"Saving repository changes");
            await repoSettings.SaveAsync(Config.CancellationToken);

            Logger.LogInformation($"Deleted project {projectId}");
        }
    }
}
