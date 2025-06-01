using OwlCore.Diagnostics;
using OwlCore.Nomad.Kubo;
using OwlCore.Nomad.Kubo.Events;
using OwlCore.Storage;
using System.CommandLine;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.User
{
    /// <summary>
    /// Command to create a new user.
    /// </summary>
    public class WacsdkUserCreateCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WacsdkUserCreateCommand"/> class.
        /// </summary>
        public WacsdkUserCreateCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> nameOption, Option<string> descriptionOption)
            : base(name: "create", description: "Creates a new user.")
        {
            var knownIdOption = new Option<string>(
                name: "--known-id",
                () => Guid.NewGuid().ToString(),
                description: "A known ID of the user to create.");

            AddOption(nameOption);
            AddOption(descriptionOption);
            AddOption(repoOption);
            AddOption(knownIdOption);

            this.SetHandler(InvokeAsync, repoOption, knownIdOption, nameOption, descriptionOption);

            Config = config;
        }

        public WacsdkCommandConfig Config { get; }

        /// <summary>
        /// Handles the command.
        /// </summary>
        /// <param name="repoId">The ID of the repository.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task InvokeAsync(string repoId, string knownId, string name, string description)
        {
            var thisRepoStorage = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false);

            Logger.LogInformation($"Getting repo store with ID {repoId} at {thisRepoStorage.GetType().Name} {thisRepoStorage.Id}");
            var repoSettings = new WacsdkNomadSettings(thisRepoStorage);
            await repoSettings.LoadAsync();

            var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);

            var createdUser = await repositoryContainer.UserRepository.CreateAsync(new(KnownId: knownId), Config.CancellationToken);
            Logger.LogInformation($"Created user with ID {createdUser.Id} via known ID {knownId}");

            Logger.LogInformation($"Setting name and description");

            if (!string.IsNullOrWhiteSpace(name))
                await createdUser.UpdateNameAsync(name, Config.CancellationToken);

            if (!string.IsNullOrWhiteSpace(description))
                await createdUser.UpdateDescriptionAsync(description, Config.CancellationToken);

            Logger.LogInformation($"Publishing local event stream to ipns");
            await createdUser.PublishLocalAsync<ModifiableUser, ValueUpdateEvent>(Config.CancellationToken);

            Logger.LogInformation($"Publishing roaming value to ipns");
            await createdUser.PublishRoamingAsync<ModifiableUser, ValueUpdateEvent, WindowsAppCommunity.Sdk.Models.User>(Config.CancellationToken);

            Logger.LogInformation($"Saving repository keys");
            await repoSettings.SaveAsync(Config.CancellationToken);
        }
    }
}
