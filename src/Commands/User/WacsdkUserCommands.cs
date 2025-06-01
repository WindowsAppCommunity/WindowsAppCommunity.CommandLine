using System.CommandLine;
using WindowsAppCommunity.CommandLine.User.Name;
using WindowsAppCommunity.CommandLine.User.Description;
using WindowsAppCommunity.CommandLine.User.ExtendedDescription;
using WindowsAppCommunity.CommandLine.User.IsUnlisted;
using WindowsAppCommunity.CommandLine.User.ForgetMe;
using WindowsAppCommunity.CommandLine.User.Connections;
using WindowsAppCommunity.CommandLine.User.Images;
using WindowsAppCommunity.CommandLine.User.Links;
using WindowsAppCommunity.CommandLine.User.PublisherRoles;
using WindowsAppCommunity.CommandLine.User.ProjectRoles;

namespace WindowsAppCommunity.CommandLine.User;

/// <summary>
/// Wacsdk user management commands.
/// </summary>
public class WacsdkUserCommands : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WacsdkUserCommands"/> class.
    /// </summary>
    public WacsdkUserCommands(WacsdkCommandConfig config, Option<string> repoOption)
        : base("user", "Manages users in a WACSDK repository.")
    {
        var userIdOption = new Option<string>("--user-id", "The ID of the user.") { IsRequired = true };
        var valueOption = new Option<string>("--value", "The value to set.");
        var boolValueOption = new Option<bool>("--value", "The boolean value to set.");
        var nullableBoolValueOption = new Option<bool?>("--value", "The nullable boolean value to set.");
        var connectionIdOption = new Option<string>("--connection-id", "The ID of the connection.");
        var connectionValueOption = new Option<string>("--connection-value", "The value of the connection string.");
        var imagePathOption = new Option<string>("--image-path", "The path to the image file.");
        var imageNameOption = new Option<string?>("--image-name", "Optional custom name for the image. If not provided, the id of the given image will be used.");
        var requiredImageIdOption = new Option<string>("--image-id", "The ID of the image to remove.") { IsRequired = true };
        var optionalImageIdOption = new Option<string?>("--image-id", "The ID of the image to add. If not provided, the id of the given image will be used.");
        var linkIdOption = new Option<string>("--link-id", "The ID of the link.");
        var nameOption = new Option<string>("--name", "The name of the entity.");
        var urlOption = new Option<string>("--url", "The URL of the link.");
        var descriptionOption = new Option<string>("--description", "The description of the entity.");
        var publisherIdOption = new Option<string>("--publisher-id", "The ID of the publisher.");
        var publisherRoleIdOption = new Option<string>("--publisher-id", "The ID of the publisher role.");
        var projectIdOption = new Option<string>("--project-id", "The ID of the project.");
        var projectRoleIdOption = new Option<string>("--project-id", "The ID of the project role.");
        var roleIdOption = new Option<string>("--role-id", "The ID of the role.");
        var roleNameOption = new Option<string>("--role-name", "The name of the role.");
        var roleDescriptionOption = new Option<string>("--role-description", "The description of the role.");
        
        
        // Add high-level user operations
        AddCommand(new WacsdkUserGetCommand(config, repoOption));
        AddCommand(new WacsdkUserCreateCommand(config, repoOption, nameOption, descriptionOption));
        AddCommand(new WacsdkUserListCommand(config, repoOption)); 
        
        // Add entity property management commands
        AddCommand(new EntityNameCommand(config, repoOption, userIdOption, valueOption));
        AddCommand(new EntityDescription(config, repoOption, userIdOption, valueOption));
        AddCommand(new EntityExtendedDescription(config, repoOption, userIdOption, valueOption));
        AddCommand(new EntityIsUnlistedCommand(config, repoOption, userIdOption, boolValueOption));
        AddCommand(new EntityForgetMeCommand(config, repoOption, userIdOption, nullableBoolValueOption));
        
        // Add collection management commands
        AddCommand(new ConnectionsCommand(config, repoOption, userIdOption, connectionIdOption, connectionValueOption));
        AddCommand(new EntityImagesCommand(config, repoOption, userIdOption, imagePathOption, imageNameOption, optionalImageIdOption, requiredImageIdOption));
        AddCommand(new LinksCommand(config, repoOption, userIdOption, linkIdOption, nameOption, urlOption, descriptionOption));
        AddCommand(new PublisherRolesCommand(config, repoOption, userIdOption, publisherIdOption, publisherRoleIdOption, roleIdOption, roleNameOption, roleDescriptionOption));
        AddCommand(new ProjectRolesCommand(config, repoOption, userIdOption, projectIdOption, projectRoleIdOption, roleIdOption, roleNameOption, roleDescriptionOption));
    }
}