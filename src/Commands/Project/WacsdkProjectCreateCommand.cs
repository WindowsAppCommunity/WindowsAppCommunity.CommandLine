using OwlCore.Diagnostics;
using OwlCore.Nomad.Kubo;
using OwlCore.Nomad.Kubo.Events;
using OwlCore.Storage;
using System.CommandLine;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Project
{
    /// <summary>
    /// Command to create a new project.
    /// </summary>
    public class WacsdkProjectCreateCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WacsdkProjectCreateCommand"/> class.
        /// </summary>
        public WacsdkProjectCreateCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> nameOption, Option<string> descriptionOption)
            : base(name: "create", description: "Creates a new project.")
        {
            Config = config;
            var knownIdOption = new Option<string>(
                name: "--known-id",
                () => Guid.NewGuid().ToString(),
                description: "A known ID of the project to create.");

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

            var createdProject = await repositoryContainer.ProjectRepository.CreateAsync(new(KnownId: knownId), Config.CancellationToken);
            Logger.LogInformation($"Created project with ID {createdProject.Id} via known ID {knownId}");

            Logger.LogInformation($"Setting name and description");

            await createdProject.UpdateNameAsync(name, Config.CancellationToken);
            await createdProject.UpdateDescriptionAsync(description, Config.CancellationToken);

            Logger.LogInformation($"Publishing local event stream to ipns");
            await createdProject.PublishLocalAsync<ModifiableProject, ValueUpdateEvent>(Config.CancellationToken);

            Logger.LogInformation($"Publishing roaming value to ipns");
            await createdProject.PublishRoamingAsync<ModifiableProject, ValueUpdateEvent, WindowsAppCommunity.Sdk.Models.Project>(Config.CancellationToken);

            Logger.LogInformation($"Saving repository keys");
            await repoSettings.SaveAsync(Config.CancellationToken);
        }
    }
}
