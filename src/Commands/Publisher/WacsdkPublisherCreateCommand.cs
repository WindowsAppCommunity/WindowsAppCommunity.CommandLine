using OwlCore.Diagnostics;
using OwlCore.Nomad.Kubo;
using OwlCore.Nomad.Kubo.Events;
using OwlCore.Storage;
using System.CommandLine;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Publisher
{
    /// <summary>
    /// Command to create a new publisher.
    /// </summary>
    public class WacsdkPublisherCreateCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WacsdkPublisherCreateCommand"/> class.
        /// </summary>
        public WacsdkPublisherCreateCommand(WacsdkCommandConfig config, Option<string> repoOption)
            : base(name: "create", description: "Creates a new publisher.")
        {
            Config = config;
            var knownIdOption = new Option<string>(
                name: "--known-id",
                () => Guid.NewGuid().ToString(),
                description: "A known ID of the publisher to create.");

            var nameOption = new Option<string>(
                name: "--name",
                description: "The name of the publisher.")
            {
                IsRequired = true
            };

            var descriptionOption = new Option<string>(
                name: "--description",
                description: "The description of the publisher.")
            {
                IsRequired = true
            };

            AddOption(nameOption);
            AddOption(descriptionOption);
            AddOption(repoOption);
            AddOption(knownIdOption);

            this.SetHandler(InvokeAsync, repoOption, knownIdOption, nameOption, descriptionOption);
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
        public async Task InvokeAsync(string repoId, string knownId, string name, string description)
        {
            var thisRepoStorage = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false);

            Logger.LogInformation($"Getting repo store with ID {repoId} at {thisRepoStorage.GetType().Name} {thisRepoStorage.Id}");
            var repoSettings = new WacsdkNomadSettings(thisRepoStorage);
            await repoSettings.LoadAsync();

            var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);

            var createdPublisher = await repositoryContainer.PublisherRepository.CreateAsync(new(KnownId: knownId), Config.CancellationToken);
            Logger.LogInformation($"Created publisher with ID {createdPublisher.Id} via known ID {knownId}");

            Logger.LogInformation($"Setting name and description");

            await createdPublisher.UpdateNameAsync(name, Config.CancellationToken);
            await createdPublisher.UpdateDescriptionAsync(description, Config.CancellationToken);

            Logger.LogInformation($"Publishing local event stream to ipns");
            await createdPublisher.PublishLocalAsync<ModifiablePublisher, ValueUpdateEvent>(Config.CancellationToken);

            Logger.LogInformation($"Publishing roaming value to ipns");
            await createdPublisher.PublishRoamingAsync<ModifiablePublisher, ValueUpdateEvent, WindowsAppCommunity.Sdk.Models.Publisher>(Config.CancellationToken);

            Logger.LogInformation($"Saving repository keys");
            await repoSettings.SaveAsync(Config.CancellationToken);
        }
    }
}
