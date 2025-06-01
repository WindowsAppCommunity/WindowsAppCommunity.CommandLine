using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.Project.Images;

/// <summary>
/// Wacsdk entity images command for Project.
/// </summary>
public class EntityImagesCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityImagesCommand"/> class.
    /// </summary>
    public EntityImagesCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> imagePathOption, Option<string?> optionalImageIdOption, Option<string> requiredImageIdOption, Option<string?> imageNameOption)
        : base("images", "Manages the images of the Project.")
    {
        AddCommand(new GetImagesCommand(config, repoOption, idOption));
        AddCommand(new AddImageCommand(config, repoOption, idOption, imagePathOption, optionalImageIdOption, imageNameOption));
        AddCommand(new RemoveImageCommand(config, repoOption, idOption, requiredImageIdOption));
    }
}