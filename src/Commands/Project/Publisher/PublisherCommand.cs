using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.Project.Publisher;

/// <summary>
/// Wacsdk publisher command.
/// </summary>
public class PublisherCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublisherCommand"/> class.
    /// </summary>
    public PublisherCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> projectIdOption, Option<string> publisherIdOption)
        : base("publisher", "Publisher get and set commands.")
    {
        this.SetHandler(InvokeAsync);

        AddCommand(new GetPublisherCommand(config, repoOption, projectIdOption));
        AddCommand(new SetPublisherCommand(config, repoOption, projectIdOption, publisherIdOption));
    }

    public async Task InvokeAsync()
    {
        // Default behavior when no subcommand is specified
    }
}