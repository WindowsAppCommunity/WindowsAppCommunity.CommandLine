using System.CommandLine;
using OwlCore.Diagnostics;
using OwlCore.Storage;

namespace WindowsAppCommunity.CommandLine.Repo
{
    /// <summary>
    /// Wacsdk repo delete command.
    /// </summary>
    public class WacsdkRepoDeleteCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WacsdkRepoDeleteCommand"/> class.
        /// </summary>
        public WacsdkRepoDeleteCommand(WacsdkCommandConfig config, Option<string> idOption)
            : base("delete", "Deletes a WACSDK repository.")
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
        /// <param name="id">The ID of the repository.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task InvokeAsync(string id)
        {
            var existingItem = await Config.RepositoryStorage.GetFirstByNameAsync(id);

            await Config.RepositoryStorage.DeleteAsync(existingItem);
            Logger.LogInformation($"Deleted repo store with ID {id} at {existingItem.Id}");
        }
    }
}
