using System.CommandLine;
using Ipfs.CoreApi;
using OwlCore.Diagnostics;
using OwlCore.Kubo;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.User
{
    /// <summary>
    /// Wacsdk user get command.
    /// </summary>
    public class WacsdkUserGetCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WacsdkUserGetCommand"/> class.
        /// </summary>
        public WacsdkUserGetCommand(WacsdkCommandConfig config, Option<string> repoOption)
            : base("get", "Gets users from a WACSDK repository.")
        {
            var userIdOption = new Option<string>(
                name: "--user-id",
                description: "The ID of the user to get.")
            {
                IsRequired = true
            };

            AddOption(repoOption);
            AddOption(userIdOption);

            this.SetHandler(InvokeAsync, repoOption, userIdOption);

            Config = config;
        }

        public WacsdkCommandConfig Config { get; }

        /// <summary>
        /// Handles the command.
        /// </summary>
        /// <param name="repoId">The ID of the repository.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task InvokeAsync(string repoId, string userId)
        {
            var thisRepoStorage = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false);

            Logger.LogInformation($"Getting repo store with ID {repoId} at {thisRepoStorage.GetType().Name} {thisRepoStorage.Id}");
            var repoSettings = new WacsdkNomadSettings(thisRepoStorage);
            await repoSettings.LoadAsync();

            var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);

            Logger.LogInformation($"Getting user {userId}");
            var user = await repositoryContainer.UserRepository.GetAsync(userId, Config.CancellationToken);
            {
                Logger.LogInformation($"{nameof(user.Id)}: {user.Id}");
                Logger.LogInformation($"{nameof(user.Name)}: {user.Name}");
                Logger.LogInformation($"{nameof(user.Description)}: {user.Description}");
                Logger.LogInformation($"{nameof(user.ExtendedDescription)}: {user.ExtendedDescription}");
                
                Logger.LogInformation($"{nameof(user.Links)}:");
                foreach (var link in user.Links)
                {
                    Logger.LogInformation($"- {nameof(link.Id)}: {link.Id}");
                    Logger.LogInformation($"  {nameof(link.Name)}: {link.Name}");
                    Logger.LogInformation($"  {nameof(link.Description)}: {link.Description}");
                    Logger.LogInformation($"  {nameof(link.Url)}: {link.Url}");
                }

                Logger.LogInformation($"{nameof(user.GetImageFilesAsync)}:");
                await foreach (var image in user.GetImageFilesAsync(Config.CancellationToken))
                {
                    Logger.LogInformation($"- {nameof(image.Id)}: {image.Id}");
                    Logger.LogInformation($"  {nameof(image.Name)}: {image.Name}");

                    var cid = await image.GetCidAsync(Config.Client, new AddFileOptions { Pin = Config.KuboOptions.ShouldPin }, Config.CancellationToken);
                    Logger.LogInformation($"  {nameof(StorableKuboExtensions.GetCidAsync)}: {cid}");
                    Logger.LogInformation($"  Type: {image.GetType()}");
                }

                Logger.LogInformation($"{nameof(user.GetConnectionsAsync)}:");
                await foreach (var connection in user.GetConnectionsAsync(Config.CancellationToken))
                {
                    Logger.LogInformation($"- {nameof(connection.Id)}: {connection.Id}");
                    Logger.LogInformation($"  {nameof(connection.GetValueAsync)}: {await connection.GetValueAsync(Config.CancellationToken)}");
                }

                Logger.LogInformation($"{nameof(user.GetPublishersAsync)}:");
                await foreach (var publisher in user.GetPublishersAsync(Config.CancellationToken))
                {
                    Logger.LogInformation($"- {nameof(publisher.Id)}: {publisher.Id}");
                    Logger.LogInformation($"  {nameof(publisher.Name)}: {publisher.Name}");
                    Logger.LogInformation($"  {nameof(publisher.Role)}:");
                    Logger.LogInformation($"    {nameof(publisher.Role.Id)}: {publisher.Role.Id}");
                    Logger.LogInformation($"    {nameof(publisher.Role.Name)}: {publisher.Role.Name}");
                    Logger.LogInformation($"    {nameof(publisher.Role.Description)}: {publisher.Role.Description}");
                }

                Logger.LogInformation($"{nameof(user.GetProjectsAsync)}:");
                await foreach (var project in user.GetProjectsAsync(Config.CancellationToken))
                {
                    Logger.LogInformation($"- {nameof(project.Id)}: {project.Id}");
                    Logger.LogInformation($"  {nameof(project.Name)}: {project.Name}");
                    Logger.LogInformation($"  {nameof(project.Role)}:");
                    Logger.LogInformation($"    {nameof(project.Role.Id)}: {project.Role.Id}");
                    Logger.LogInformation($"    {nameof(project.Role.Name)}: {project.Role.Name}");
                    Logger.LogInformation($"    {nameof(project.Role.Description)}: {project.Role.Description}");
                }
            }
        }
    }
}
