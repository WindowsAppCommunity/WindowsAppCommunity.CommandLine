using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.Publisher.ChildPublishers;

/// <summary>
/// Wacsdk child publishers command for Publisher.
/// </summary>
public class ChildPublishersCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChildPublishersCommand"/> class.
    /// </summary>
    public ChildPublishersCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> publisherIdOption)
        : base("child-publishers", "Manages the child publishers of the Publisher.")
    {
        AddCommand(new GetChildPublishersCommand(config, repoOption, idOption));
        AddCommand(new AddChildPublisherCommand(config, repoOption, idOption, publisherIdOption));
        AddCommand(new RemoveChildPublisherCommand(config, repoOption, idOption, publisherIdOption));
    }
}