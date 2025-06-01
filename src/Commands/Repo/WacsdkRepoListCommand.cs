using System.CommandLine;
using OwlCore.Diagnostics;
using OwlCore.Storage;

namespace WindowsAppCommunity.CommandLine.Repo
{
    /// <summary>
    /// Wacsdk repo list command.
    /// </summary>
    public class WacsdkRepoListCommand : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WacsdkRepoListCommand"/> class.
        /// </summary>
        public WacsdkRepoListCommand(WacsdkCommandConfig config)
            : base("list", "Lists WACSDK repositories.")
        {
            this.SetHandler(InvokeAsync);
            Config = config;
        }

        public WacsdkCommandConfig Config { get; }

        /// <summary>
        /// Handles the command.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task InvokeAsync()
        {
            Logger.LogInformation($"Listing repositories");
            await foreach (var item in Config.RepositoryStorage.GetFoldersAsync(Config.CancellationToken))
            {
                Logger.LogInformation(item.Id);
            }
        }
    }
}
