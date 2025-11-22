using System.CommandLine;
using WindowsAppCommunity.CommandLine.Blog.PostPage;

namespace WindowsAppCommunity.CommandLine.Blog
{
    /// <summary>
    /// Command aggregator for blog generation scenarios.
    /// Registers Post/Page, Pages, and Site scenario commands.
    /// </summary>
    public class WacsdkBlogCommands : Command
    {
        /// <summary>
        /// Initialize blog commands aggregator.
        /// Registers all blog generation scenario subcommands.
        /// </summary>
        public WacsdkBlogCommands()
            : base("blog", "Blog generation commands")
        {
            // Register Post/Page scenario
            AddCommand(new PostPageCommand());
            
            // Register Pages scenario
            AddCommand(new PagesCommand());
            
            // Future: Register Site scenario
            // AddCommand(new SiteCommand());
        }
    }
}
