using System.CommandLine;
using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Project
{
    /// <summary>
    /// Wacsdk project list command.
    /// </summary>
    public class WacsdkProjectListCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WacsdkProjectListCommand"/> class.
        /// </summary>
        /// <param name="idOption">The option for the repository ID.</param>
        public WacsdkProjectListCommand(WacsdkCommandConfig config, Option<string> idOption)
            : base("list", "Lists WACSDK projects from a repository.")
        {
            Config = config;
            AddOption(idOption);
            this.SetHandler(InvokeAsync, idOption);
        }

        /// <summary>
        /// Gets the configuration for the command.
        /// </summary>
        public WacsdkCommandConfig Config { get; }

        /// <summary>
        /// Handles the command.
        /// </summary>
        /// <param name="repoId">The ID of the repository.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task InvokeAsync(string repoId)
        {
            var thisRepoStorage = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false);

            Logger.LogInformation($"Getting repo store with ID {repoId} at {thisRepoStorage.GetType().Name} {thisRepoStorage.Id}");
            var repoSettings = new WacsdkNomadSettings(thisRepoStorage);
            await repoSettings.LoadAsync();

            var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);

            Logger.LogInformation($"Listing projects for repository {repoId}");
            await foreach (var project in repositoryContainer.ProjectRepository.GetAsync(Config.CancellationToken))
            {
                Logger.LogInformation($"{nameof(project.Id)}: {project.Id}");
                Logger.LogInformation($"{nameof(project.Name)}: {project.Name}");
            }
            Logger.LogInformation($"Finished listing projects for repository {repoId}");
        }
    }
}
