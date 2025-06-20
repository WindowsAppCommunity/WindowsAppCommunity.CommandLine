using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.Publisher.ParentPublishers;

/// <summary>
/// Wacsdk parent publishers command for Publisher.
/// </summary>
public class ParentPublishersCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParentPublishersCommand"/> class.
    /// </summary>
    public ParentPublishersCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> publisherIdOption, Option<string> roleIdOption, Option<string> roleNameOption, Option<string> roleDescriptionOption)
        : base("parent-publishers", "Manages the parent publishers of the Publisher.")
    {
        AddCommand(new GetParentPublishersCommand(config, repoOption, idOption));
        AddCommand(new AddParentPublisherCommand(config, repoOption, idOption, publisherIdOption, roleIdOption, roleNameOption, roleDescriptionOption));
        AddCommand(new RemoveParentPublisherCommand(config, repoOption, idOption, publisherIdOption));
    }
}