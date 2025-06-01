using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.Repo
{
    /// <summary>
    /// Wacsdk repo commands.
    /// </summary>
    public class WacsdkRepoCommands : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WacsdkRepoCommands"/> class.
        /// </summary>
        public WacsdkRepoCommands(WacsdkCommandConfig config)
            : base("repo", "Manage WACSDK repositories.")
        {
            var idOption = new Option<string>(
                name: "--id",
                description: "The ID of the repository.")
            {
                IsRequired = true,
            };

            AddCommand(new WacsdkRepoListCommand(config));
            AddCommand(new WacsdkRepoGetCommand(config, idOption));
            AddCommand(new WacsdkRepoCreateCommand(config, idOption));
            AddCommand(new WacsdkRepoDeleteCommand(config, idOption));
        }
    }
}
