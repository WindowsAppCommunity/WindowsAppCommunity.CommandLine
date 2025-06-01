using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.Publisher.Links;

/// <summary>
/// Wacsdk links command for Publisher.
/// </summary>
public class LinksCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LinksCommand"/> class.
    /// </summary>
    public LinksCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> linkIdOption, Option<string> linkNameOption, Option<string> linkUrlOption, Option<string> linkDescriptionOption)
        : base("links", "Manages the links of the Publisher.")
    {
        AddCommand(new GetLinkCommand(config, repoOption, idOption));
        AddCommand(new AddLinkCommand(config, repoOption, idOption, linkIdOption, linkNameOption, linkUrlOption, linkDescriptionOption));
        AddCommand(new RemoveLinkCommand(config, repoOption, idOption, linkNameOption));
    }
}