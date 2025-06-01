using System.CommandLine;
using Ipfs.CoreApi;
using OwlCore.Diagnostics;
using OwlCore.Kubo;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Project
{
    /// <summary>
    /// Wacsdk project get command.
    /// </summary>
    public class WacsdkProjectGetCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WacsdkProjectGetCommand"/> class.
        /// </summary>
        public WacsdkProjectGetCommand(WacsdkCommandConfig config, Option<string> repoOption)
            : base("get", "Gets projects from a WACSDK repository.")
        {
            var projectIdOption = new Option<string>(
                name: "--project-id",
                description: "The ID of the project to get.")
            {
                IsRequired = true
            };

            AddOption(repoOption);
            AddOption(projectIdOption);
            this.SetHandler(InvokeAsync, repoOption, projectIdOption);

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
        /// <param name="projectId">The ID of the project.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task InvokeAsync(string repoId, string projectId)
        {
            var thisRepoStorage = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false);

            Logger.LogInformation($"Getting repo store with ID {repoId} at {thisRepoStorage.GetType().Name} {thisRepoStorage.Id}");
            var repoSettings = new WacsdkNomadSettings(thisRepoStorage);
            await repoSettings.LoadAsync();

            var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);

            Logger.LogInformation($"Getting project {projectId}");
            var project = await repositoryContainer.ProjectRepository.GetAsync(projectId, Config.CancellationToken);
            {
                Logger.LogInformation($"{nameof(project.Id)}: {project.Id}");
                Logger.LogInformation($"{nameof(project.Name)}: {project.Name}");
                Logger.LogInformation($"{nameof(project.Description)}: {project.Description}");
                Logger.LogInformation($"{nameof(project.ExtendedDescription)}: {project.ExtendedDescription}");
                Logger.LogInformation($"{nameof(project.AccentColor)}: {project.AccentColor}");
                Logger.LogInformation($"{nameof(project.Category)}: {project.Category}");

                Logger.LogInformation($"{nameof(project.Features)}:");
                foreach (var feature in project.Features)
                {
                    Logger.LogInformation($"  {nameof(feature)}: {feature}");
                }

                Logger.LogInformation($"{nameof(project.Links)}:");
                foreach (var link in project.Links)
                {
                    Logger.LogInformation($"- {nameof(link.Id)}: {link.Id}");
                    Logger.LogInformation($"  {nameof(link.Name)}: {link.Name}");
                    Logger.LogInformation($"  {nameof(link.Description)}: {link.Description}");
                    Logger.LogInformation($"  {nameof(link.Url)}: {link.Url}");
                }

                Logger.LogInformation($"{nameof(project.GetImageFilesAsync)}:");
                await foreach (var image in project.GetImageFilesAsync(Config.CancellationToken))
                {
                    Logger.LogInformation($"- {nameof(image.Id)}: {image.Id}");
                    Logger.LogInformation($"  {nameof(image.Name)}: {image.Name}");

                    var cid = await image.GetCidAsync(Config.Client, new AddFileOptions { Pin = Config.KuboOptions.ShouldPin }, Config.CancellationToken);
                    Logger.LogInformation($"  {nameof(StorableKuboExtensions.GetCidAsync)}: {cid}");
                    Logger.LogInformation($"  Type: {image.GetType()}");
                }

                Logger.LogInformation($"{nameof(project.GetConnectionsAsync)}:");
                await foreach (var connection in project.GetConnectionsAsync(Config.CancellationToken))
                {
                    Logger.LogInformation($"- {nameof(connection.Id)}: {connection.Id}");
                    Logger.LogInformation($"  {nameof(connection.GetValueAsync)}: {await connection.GetValueAsync(Config.CancellationToken)}");
                }

                Logger.LogInformation($"{nameof(project.GetUsersAsync)}:");
                await foreach (var user in project.GetUsersAsync(Config.CancellationToken))
                {
                    Logger.LogInformation($"- {nameof(user.Id)}: {user.Id}");
                    Logger.LogInformation($"  {nameof(user.Name)}: {user.Name}");
                    Logger.LogInformation($"  {nameof(user.Role)}:");
                    Logger.LogInformation($"    {nameof(user.Role.Id)}: {user.Role.Id}");
                    Logger.LogInformation($"    {nameof(user.Role.Name)}: {user.Role.Name}");
                    Logger.LogInformation($"    {nameof(user.Role.Description)}: {user.Role.Description}");
                }

                Logger.LogInformation($"{nameof(project.GetPublisherAsync)}:");
                var publisher = await project.GetPublisherAsync(Config.CancellationToken);
                if (publisher is not null)
                {
                    Logger.LogInformation($"- {nameof(publisher.Id)}: {publisher.Id}");
                    Logger.LogInformation($"  {nameof(publisher.Name)}: {publisher.Name}");
                }

                Logger.LogInformation($"{nameof(project.Dependencies)}:");
                await foreach (var dependency in project.Dependencies.GetProjectsAsync(Config.CancellationToken))
                {
                    Logger.LogInformation($"- {nameof(dependency.Id)}: {dependency.Id}");
                    Logger.LogInformation($"  {nameof(dependency.Name)}: {dependency.Name}");
                }
            }
        }
    }
}
