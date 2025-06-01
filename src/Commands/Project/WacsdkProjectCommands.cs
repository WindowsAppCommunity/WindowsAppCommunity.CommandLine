using System.CommandLine;
using WindowsAppCommunity.CommandLine.Project.Name;
using WindowsAppCommunity.CommandLine.Project.Description;
using WindowsAppCommunity.CommandLine.Project.ExtendedDescription;
using WindowsAppCommunity.CommandLine.Project.IsUnlisted;
using WindowsAppCommunity.CommandLine.Project.ForgetMe;
using WindowsAppCommunity.CommandLine.Project.Connections;
using WindowsAppCommunity.CommandLine.Project.Images;
using WindowsAppCommunity.CommandLine.Project.Links;
using WindowsAppCommunity.CommandLine.Project.UserRoles;
using WindowsAppCommunity.CommandLine.Project.AccentColor;
using WindowsAppCommunity.CommandLine.Project.Category;
using WindowsAppCommunity.CommandLine.Project.Features;
using WindowsAppCommunity.CommandLine.Project.Publisher;

namespace WindowsAppCommunity.CommandLine.Project;

/// <summary>
/// Wacsdk project management commands.
/// </summary>
public class WacsdkProjectCommands : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WacsdkProjectCommands"/> class.
    /// </summary>
    public WacsdkProjectCommands(WacsdkCommandConfig config, Option<string> repoOption)
        : base("project", "Manages projects in a WACSDK repository.")
    {
        var projectIdOption = new Option<string>("--project-id", "The ID of the project.") { IsRequired = true };
        var valueOption = new Option<string>("--value", "The value to set.");
        var boolValueOption = new Option<bool>("--value", "The boolean value to set.");
        var nullableBoolValueOption = new Option<bool?>("--value", "The nullable boolean value to set.");
        var connectionIdOption = new Option<string>("--connection-id", "The ID of the connection.");
        var connectionValueOption = new Option<string>("--connection-value", "The value of the connection.");
        var imagePathOption = new Option<string>("--image-path", "The path to the image file.");
        var imageNameOption = new Option<string?>("--image-name", "Optional custom name for the image. If not provided, the id of the given image will be used.");
        var requiredImageIdOption = new Option<string>("--image-id", "The ID of the image to remove.") { IsRequired = true };
        var optionalImageIdOption = new Option<string?>("--image-id", "The ID of the image to add. If not provided, the id of the given image will be used.");
        var linkIdOption = new Option<string>("--link-id", "The ID of the link.");
        var nameOption = new Option<string>("--name", "The name of the entity.");
        var urlOption = new Option<string>("--url", "The URL of the link.");
        var descriptionOption = new Option<string>("--description", "The description of the entity.");
        var userIdOption = new Option<string>("--user-id", "The ID of the user.");
        var roleIdOption = new Option<string>("--role-id", "The ID of the role.");
        var roleNameOption = new Option<string>("--role-name", "The name of the role.");
        var roleDescriptionOption = new Option<string>("--role-description", "The description of the role.");
        var colorOption = new Option<string>("--color", "The accent color value.");
        var categoryOption = new Option<string>("--category", "The category value.");
        var featureOption = new Option<string>("--feature", "The feature to add or remove.");
        var publisherIdOption = new Option<string>("--publisher-id", "The publisher ID.");

        // Add high-level project operations
        AddCommand(new WacsdkProjectGetCommand(config, repoOption));
        AddCommand(new WacsdkProjectCreateCommand(config, repoOption, nameOption, descriptionOption));
        AddCommand(new WacsdkProjectListCommand(config, repoOption));

        // Add entity property management commands
        AddCommand(new EntityNameCommand(config, repoOption, projectIdOption, valueOption));
        AddCommand(new EntityDescription(config, repoOption, projectIdOption, valueOption));
        AddCommand(new EntityExtendedDescription(config, repoOption, projectIdOption, valueOption));
        AddCommand(new EntityIsUnlistedCommand(config, repoOption, projectIdOption, boolValueOption));
        AddCommand(new EntityForgetMeCommand(config, repoOption, projectIdOption, nullableBoolValueOption));
        
        // Add collection management commands
        AddCommand(new ConnectionsCommand(config, repoOption, projectIdOption, connectionIdOption, connectionValueOption));
        AddCommand(new EntityImagesCommand(config, repoOption, projectIdOption, imagePathOption, optionalImageIdOption, requiredImageIdOption, imageNameOption));
        AddCommand(new LinksCommand(config, repoOption, projectIdOption, linkIdOption, nameOption, urlOption, descriptionOption));
        AddCommand(new UserRolesCommand(config, repoOption, projectIdOption, userIdOption, roleIdOption, roleNameOption, roleDescriptionOption));
        AddCommand(new AccentColorCommand(config, repoOption, projectIdOption, colorOption));
        
        // Add project-specific commands
        AddCommand(new CategoryCommand(config, repoOption, projectIdOption, categoryOption));
        AddCommand(new FeaturesCommand(config, repoOption, projectIdOption, featureOption));
        AddCommand(new PublisherCommand(config, repoOption, projectIdOption, publisherIdOption));
    }
}