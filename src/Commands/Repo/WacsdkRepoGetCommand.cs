using System.CommandLine;
using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Repo
{
    /// <summary>
    /// Wacsdk repo get command.
    /// </summary>
    public class WacsdkRepoGetCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WacsdkRepoGetCommand"/> class.
        /// </summary>
        public WacsdkRepoGetCommand(WacsdkCommandConfig config, Option<string> idOption)
            : base("get", "Gets a WACSDK repository by ID.")
        {
            AddOption(idOption);
            this.SetHandler(InvokeAsync, idOption);

            Config = config;
        }

        /// <summary>
        /// Gets the configuration for the command.
        /// </summary>
        public WacsdkCommandConfig Config { get; }

        /// <summary>
        /// Handles the command.
        /// </summary>
        /// <param name="id">The ID of the repository.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task InvokeAsync(string id)
        {
            var thisRepoStorage = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(id, overwrite: false);

            Logger.LogInformation($"Getting repo store with ID {id} at {thisRepoStorage.GetType().Name} {thisRepoStorage.Id}");
            var repoSettings = new WacsdkNomadSettings(thisRepoStorage);
            await repoSettings.LoadAsync();

            var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);

            var savedMissingFromRepoConfigs = repositoryContainer.PublisherRepository.ManagedConfigs.Where(x => repoSettings.ManagedPublisherConfigs.Any(y => x.RoamingId != y.RoamingId)).ToList();
            var repoMissingFromSavedConfigs = repoSettings.ManagedPublisherConfigs.Where(x => repositoryContainer.PublisherRepository.ManagedConfigs.Any(y => x.RoamingId != y.RoamingId)).ToList();   

            foreach (var item in savedMissingFromRepoConfigs)
                repositoryContainer.PublisherRepository.ManagedConfigs.Add(item);             

            Logger.LogInformation($"List users for repository {id}");
            await foreach (var user in repositoryContainer.UserRepository.GetAsync(Config.CancellationToken))
            {
                Logger.LogInformation(user.Id);
            }

            Logger.LogInformation($"Listing projects for repository {id}");
            await foreach (var project in repositoryContainer.ProjectRepository.GetAsync(Config.CancellationToken))
            {
                Logger.LogInformation(project.Id);
            }

            Logger.LogInformation($"Listing publishers for repository {id}");
            await foreach (var publisher in repositoryContainer.PublisherRepository.GetAsync(Config.CancellationToken))
            {
                Logger.LogInformation(publisher.Id);
            }

            foreach (var item in repoMissingFromSavedConfigs)
                repoSettings.ManagedPublisherConfigs = [.. repoSettings.ManagedPublisherConfigs, item];  

            await repoSettings.SaveAsync();
            Logger.LogInformation($"Saved repo store with ID {id} at {thisRepoStorage.GetType().Name} {thisRepoStorage.Id}");
        }
    }
}
