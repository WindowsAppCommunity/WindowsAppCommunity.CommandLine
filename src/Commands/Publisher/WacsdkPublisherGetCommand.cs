using System.CommandLine;
using Ipfs.CoreApi;
using OwlCore.Diagnostics;
using OwlCore.Kubo;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Publisher
{
    /// <summary>
    /// Wacsdk publisher get command.
    /// </summary>
    public class WacsdkPublisherGetCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WacsdkPublisherGetCommand"/> class.
        /// </summary>
        public WacsdkPublisherGetCommand(WacsdkCommandConfig config, Option<string> repoOption)
            : base("get", "Gets publishers from a WACSDK repository.")
        {
            Config = config;
            var publisherIdOption = new Option<string>(
                name: "--publisher-id",
                description: "The ID of the publisher to get.")
            {
                IsRequired = true
            };

            AddOption(repoOption);
            AddOption(publisherIdOption);
            this.SetHandler(InvokeAsync, repoOption, publisherIdOption);
        }

        /// <summary>
        /// Gets the configuration for the command.
        /// </summary>
        public WacsdkCommandConfig Config { get; }

        /// <summary>
        /// Handles the command.
        /// </summary>
        /// <param name="repoId">The ID of the repository.</param>
        /// <param name="publisherId">The ID of the publisher.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task InvokeAsync(string repoId, string publisherId)
        {
            var thisRepoStorage = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false);

            Logger.LogInformation($"Getting repo store with ID {repoId} at {thisRepoStorage.GetType().Name} {thisRepoStorage.Id}");
            var repoSettings = new WacsdkNomadSettings(thisRepoStorage);
            await repoSettings.LoadAsync();

            var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);

            Logger.LogInformation($"Getting publisher {publisherId}");
            var publisher = await repositoryContainer.PublisherRepository.GetAsync(publisherId, Config.CancellationToken);
            {
                Logger.LogInformation($"{nameof(publisher.Id)}: {publisher.Id}");
                Logger.LogInformation($"{nameof(publisher.Name)}: {publisher.Name}");
                Logger.LogInformation($"{nameof(publisher.Description)}: {publisher.Description}");
                Logger.LogInformation($"{nameof(publisher.ExtendedDescription)}: {publisher.ExtendedDescription}");
                Logger.LogInformation($"{nameof(publisher.AccentColor)}: {publisher.AccentColor}");

                Logger.LogInformation($"{nameof(publisher.GetConnectionsAsync)}:");
                await foreach (var connection in publisher.GetConnectionsAsync(Config.CancellationToken))
                {
                    Logger.LogInformation($"  {nameof(connection.Id)}: {connection.Id}");
                    Logger.LogInformation($"  {nameof(connection.GetValueAsync)}: {await connection.GetValueAsync(Config.CancellationToken)}");
                }

                Logger.LogInformation($"{nameof(publisher.Links)}:");
                foreach (var link in publisher.Links)
                {
                    Logger.LogInformation($"- {nameof(link.Id)}: {link.Id}");
                    Logger.LogInformation($"  {nameof(link.Name)}: {link.Name}");
                    Logger.LogInformation($"  {nameof(link.Description)}: {link.Description}");
                    Logger.LogInformation($"  {nameof(link.Url)}: {link.Url}");
                }

                Logger.LogInformation($"{nameof(publisher.GetImageFilesAsync)}:");
                await foreach (var image in publisher.GetImageFilesAsync(Config.CancellationToken))
                {
                    Logger.LogInformation($"- {nameof(image.Id)}: {image.Id}");
                    Logger.LogInformation($"  {nameof(image.Name)}: {image.Name}");

                    var cid = await image.GetCidAsync(Config.Client, new AddFileOptions { Pin = Config.KuboOptions.ShouldPin }, Config.CancellationToken);
                    Logger.LogInformation($"  {nameof(StorableKuboExtensions.GetCidAsync)}: {cid}");
                    Logger.LogInformation($"  Type: {image.GetType()}");
                }

                Logger.LogInformation($"{nameof(publisher.GetConnectionsAsync)}:");
                await foreach (var connection in publisher.GetConnectionsAsync(Config.CancellationToken))
                {
                    Logger.LogInformation($"- {nameof(connection.Id)}: {connection.Id}");
                    Logger.LogInformation($"  {nameof(connection.GetValueAsync)}: {await connection.GetValueAsync(Config.CancellationToken)}");
                }

                Logger.LogInformation($"{nameof(publisher.GetProjectsAsync)}:");
                await foreach (var project in publisher.GetProjectsAsync(Config.CancellationToken))
                {
                    Logger.LogInformation($"- {nameof(project.Id)}: {project.Id}");
                    Logger.LogInformation($"  {nameof(project.Name)}: {project.Name}");
                }

                Logger.LogInformation($"{nameof(publisher.GetUsersAsync)}:");
                await foreach (var user in publisher.GetUsersAsync(Config.CancellationToken))
                {
                    Logger.LogInformation($"- {nameof(user.Id)}: {user.Id}");
                    Logger.LogInformation($"  {nameof(user.Name)}: {user.Name}");
                    Logger.LogInformation($"  {nameof(user.Role)}:");
                    Logger.LogInformation($"    {nameof(user.Role.Id)}: {user.Role.Id}");
                    Logger.LogInformation($"    {nameof(user.Role.Name)}: {user.Role.Name}");
                    Logger.LogInformation($"    {nameof(user.Role.Description)}: {user.Role.Description}");
                }

                Logger.LogInformation($"{nameof(publisher.ParentPublishers)}:");
                await foreach (var parentPublisher in publisher.ParentPublishers.GetPublishersAsync(Config.CancellationToken))
                {
                    Logger.LogInformation($"- {nameof(parentPublisher.Id)}: {parentPublisher.Id}");
                    Logger.LogInformation($"  {nameof(parentPublisher.Name)}: {parentPublisher.Name}");
                }

                Logger.LogInformation($"{nameof(publisher.ChildPublishers)}:");
                await foreach (var childPublisher in publisher.ChildPublishers.GetPublishersAsync(Config.CancellationToken))
                {
                    Logger.LogInformation($"- {nameof(childPublisher.Id)}: {childPublisher.Id}");
                    Logger.LogInformation($"  {nameof(childPublisher.Name)}: {childPublisher.Name}");
                }

                Logger.LogInformation($"Finished getting publisher {publisherId} from repo store with ID {repoId} at {thisRepoStorage.GetType().Name} {thisRepoStorage.Id}");
            }
        }
    }
}
