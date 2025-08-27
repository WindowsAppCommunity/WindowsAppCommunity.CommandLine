using OwlCore.Diagnostics;
using OwlCore.Storage;
using System.CommandLine;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Publisher
{
    /// <summary>
    /// Command to delete an existing publisher.
    /// </summary>
    public class WacsdkPublisherDeleteCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WacsdkPublisherDeleteCommand"/> class.
        /// </summary>
        public WacsdkPublisherDeleteCommand(WacsdkCommandConfig config, Option<string> repoOption)
            : base(name: "delete", description: "Deletes a publisher.")
        {
            var publisherIdOption = new Option<string>(
                name: "--publisher-id",
                description: "The ID of the publisher to delete.")
            {
                IsRequired = true
            };

            AddOption(repoOption);
            AddOption(publisherIdOption);
            this.SetHandler(InvokeAsync, repoOption, publisherIdOption);

            Config = config;
        }

        /// <summary>
        /// Shared command configuration.
        /// </summary>
        public WacsdkCommandConfig Config { get; }

        /// <summary>
        /// Handles the command.
        /// </summary>
        public async Task InvokeAsync(string repoId, string publisherId)
        {
            var thisRepoStorage = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false);

            Logger.LogInformation($"Getting repo store with ID {repoId} at {thisRepoStorage.GetType().Name} {thisRepoStorage.Id}");
            var repoSettings = new WacsdkNomadSettings(thisRepoStorage);
            await repoSettings.LoadAsync(Config.CancellationToken);

            var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);

            Logger.LogInformation($"Getting publisher {publisherId}");
            var publisher = (ModifiablePublisher)await repositoryContainer.PublisherRepository.GetAsync(publisherId, Config.CancellationToken);

            Logger.LogInformation($"Deleting publisher {publisherId}");
            await repositoryContainer.PublisherRepository.DeleteAsync(publisher, Config.CancellationToken);

            Logger.LogInformation($"Saving repository changes");
            await repoSettings.SaveAsync(Config.CancellationToken);

            Logger.LogInformation($"Deleted publisher {publisherId}");
        }
    }
}
