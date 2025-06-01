using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.User.Links;

/// <summary>
/// Wacsdk links command for User.
/// </summary>
public class LinksCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LinksCommand"/> class.
    /// </summary>
    public LinksCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> linkIdOption, Option<string> nameOption, Option<string> urlOption, Option<string> descriptionOption)
        : base("links", "Manages the links of the User.")
    {
        AddCommand(new GetLinkCommand(config, repoOption, idOption));
        AddCommand(new AddLinkCommand(config, repoOption, idOption, linkIdOption, nameOption, urlOption, descriptionOption));
        AddCommand(new RemoveLinkCommand(config, repoOption, idOption, linkIdOption));
    }
}