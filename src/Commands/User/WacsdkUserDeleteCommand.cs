using OwlCore.Diagnostics;
using OwlCore.Storage;
using System.CommandLine;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.User
{
    /// <summary>
    /// Command to delete an existing user.
    /// </summary>
    public class WacsdkUserDeleteCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WacsdkUserDeleteCommand"/> class.
        /// </summary>
        public WacsdkUserDeleteCommand(WacsdkCommandConfig config, Option<string> repoOption)
            : base(name: "delete", description: "Deletes a user.")
        {
            var userIdOption = new Option<string>(
                name: "--user-id",
                description: "The ID of the user to delete.")
            {
                IsRequired = true
            };

            AddOption(repoOption);
            AddOption(userIdOption);

            this.SetHandler(InvokeAsync, repoOption, userIdOption);

            Config = config;
        }

        /// <summary>
        /// Shared command configuration.
        /// </summary>
        public WacsdkCommandConfig Config { get; }

        /// <summary>
        /// Handles the command.
        /// </summary>
        public async Task InvokeAsync(string repoId, string userId)
        {
            var thisRepoStorage = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false);

            Logger.LogInformation($"Getting repo store with ID {repoId} at {thisRepoStorage.GetType().Name} {thisRepoStorage.Id}");
            var repoSettings = new WacsdkNomadSettings(thisRepoStorage);
            await repoSettings.LoadAsync(Config.CancellationToken);

            var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);

            Logger.LogInformation($"Getting user {userId}");
            var user = (ModifiableUser)await repositoryContainer.UserRepository.GetAsync(userId, Config.CancellationToken);

            Logger.LogInformation($"Deleting user {userId}");
            await repositoryContainer.UserRepository.DeleteAsync(user, Config.CancellationToken);

            Logger.LogInformation($"Saving repository changes");
            await repoSettings.SaveAsync(Config.CancellationToken);

            Logger.LogInformation($"Deleted user {userId}");
        }
    }
}
