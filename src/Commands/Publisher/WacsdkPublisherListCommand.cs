using System.CommandLine;
using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Publisher
{
    /// <summary>
    /// Wacsdk publisher list command.
    /// </summary>
    public class WacsdkPublisherListCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WacsdkPublisherListCommand"/> class.
        /// </summary>
        /// <param name="idOption">The option for the repository ID.</param>
        public WacsdkPublisherListCommand(WacsdkCommandConfig config, Option<string> idOption)
            : base("list", "Lists WACSDK publishers from a repository.")
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

            Logger.LogInformation($"Listing publishers for repository {repoId}");
            await foreach (var publisher in repositoryContainer.PublisherRepository.GetAsync(Config.CancellationToken))
            {
                Logger.LogInformation($"{nameof(publisher.Id)}: {publisher.Id}");
                Logger.LogInformation($"{nameof(publisher.Name)}: {publisher.Name}");
            }
            Logger.LogInformation($"Finished listing publishers for repository {repoId}");
        }
    }
}
