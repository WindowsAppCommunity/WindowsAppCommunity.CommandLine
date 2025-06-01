using System.CommandLine;
using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.User
{
    /// <summary>
    /// Wacsdk user list command.
    /// </summary>
    public class WacsdkUserListCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WacsdkUserListCommand"/> class.
        /// </summary>
        /// <param name="idOption">The option for the repository ID.</param>
        public WacsdkUserListCommand(WacsdkCommandConfig config, Option<string> idOption)
            : base("list", "Lists WACSDK users from a repository.")
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
        /// <param name="repoId">The ID of the repository.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task InvokeAsync(string repoId)
        {
            var thisRepoStorage = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false);

            Logger.LogInformation($"Getting repo store with ID {repoId} at {thisRepoStorage.GetType().Name} {thisRepoStorage.Id}");
            var repoSettings = new WacsdkNomadSettings(thisRepoStorage);
            await repoSettings.LoadAsync();

            var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);

            Logger.LogInformation($"Listing users for repository {repoId}");
            await foreach (var user in repositoryContainer.UserRepository.GetAsync(Config.CancellationToken))
            {
                Logger.LogInformation($"{nameof(user.Id)}: {user.Id}");
                Logger.LogInformation($"{nameof(user.Name)}: {user.Name}");
            }
            Logger.LogInformation($"Finished listing users for repository {repoId}");
        }
    }
}
