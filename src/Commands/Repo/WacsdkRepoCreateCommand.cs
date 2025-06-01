using System.CommandLine;
using OwlCore.Diagnostics;
using OwlCore.Storage;

namespace WindowsAppCommunity.CommandLine.Repo
{
    /// <summary>
    /// Wacsdk repo create command.
    /// </summary>
    public class WacsdkRepoCreateCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WacsdkRepoCreateCommand"/> class.
        /// </summary>
        public WacsdkRepoCreateCommand(WacsdkCommandConfig config, Option<string> idOption)
            : base("create", "Creates a WACSDK repository.")
        {
            Config = config;
            AddOption(idOption);
            this.SetHandler(InvokeAsync, idOption);
        }

        public WacsdkCommandConfig Config { get; }

        /// <summary>
        /// Handles the command.
        /// </summary>
        /// <param name="id">The ID of the repository.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task InvokeAsync(string id)
        {
            var thisRepoStorage = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(id, overwrite: false);
            Logger.LogInformation($"Created repo store with ID {id} at {thisRepoStorage.Id}");
        }
    }
}
