using System.CommandLine;
using CommunityToolkit.Diagnostics;
using WindowsAppCommunity.CommandLine.Publisher.Name;
using WindowsAppCommunity.CommandLine.Publisher.Description;
using WindowsAppCommunity.CommandLine.Publisher.ExtendedDescription;
using WindowsAppCommunity.CommandLine.Publisher.IsUnlisted;
using WindowsAppCommunity.CommandLine.Publisher.ForgetMe;
using WindowsAppCommunity.CommandLine.Publisher.Connections;
using WindowsAppCommunity.CommandLine.Publisher.Images;
using WindowsAppCommunity.CommandLine.Publisher.Links;
using WindowsAppCommunity.CommandLine.Publisher.UserRoles;
using WindowsAppCommunity.CommandLine.Publisher.AccentColor;
using WindowsAppCommunity.CommandLine.Publisher.Projects;
using WindowsAppCommunity.CommandLine.Publisher.ChildPublishers;
using WindowsAppCommunity.CommandLine.Publisher.ParentPublishers;

namespace WindowsAppCommunity.CommandLine.Publisher
{
    /// <summary>
    /// Wacsdk publisher commands.
    /// </summary>
    public class WacsdkPublisherCommands : Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WacsdkPublisherCommands"/> class.
        /// </summary>
        public WacsdkPublisherCommands(WacsdkCommandConfig config, Option<string> repoOption)
            : base("publisher", "Manage WACSDK publishers.")
        {
            Guard.IsNotNull(config);
            Guard.IsNotNull(repoOption);

            var publisherIdOption = new Option<string>("--publisher-id", "The ID of the publisher.") { IsRequired = true };
            var valueOption = new Option<string>("--value", "The value to set.");
            var boolValueOption = new Option<bool>("--value", "The boolean value to set.");
            var nullableBoolValueOption = new Option<bool?>("--value", "The nullable boolean value to set.");
            var connectionIdOption = new Option<string>("--connection-id", "The ID of the connection.");
            var connectionValueOption = new Option<string>("--connection-value", "The value of the connection.");
            var imagePathOption = new Option<string>("--image-path", "The path to the image file.") { IsRequired = true };
            var imageNameOption = new Option<string?>("--image-name", "Optional custom name for the image. If not provided, the id of the given image will be used.");
            var requiredImageIdOption = new Option<string>("--image-id", "The ID of the image to remove.") { IsRequired = true };
            var optionalImageIdOption = new Option<string?>("--image-id", "The ID of the image to add. If not provided, the id of the given image will be used.");
            var linkIdOption = new Option<string>("--link-id", "The ID of the link.") { IsRequired = true };
            var nameOption = new Option<string>("--name", "The name of the link.") { IsRequired = true };
            var urlOption = new Option<string>("--url", "The URL of the link.");
            var descriptionOption = new Option<string>("--description", "The description of the entity.");
            var userIdOption = new Option<string>("--user-id", "The ID of the user.") { IsRequired = true };
            var roleIdOption = new Option<string>("--role-id", "The ID of the role.") { IsRequired = true };
            var roleNameOption = new Option<string>("--role-name", "The name of the role.") { IsRequired = true };
            var roleDescriptionOption = new Option<string>("--role-description", "The description of the role.");
            var colorOption = new Option<string>("--color", "The accent color value.");
            var projectIdOption = new Option<string>("--project-id", "The ID of the project.") { IsRequired = true };
            var parentPublisherIdOption = new Option<string>("--parent-publisher-id", "The ID of the parent publisher.") { IsRequired = true };
            var childPublisherIdOption = new Option<string>("--child-publisher-id", "The ID of the child publisher.") { IsRequired = true };

            // Add high-level publisher operations
            AddCommand(new WacsdkPublisherGetCommand(config, repoOption));
            AddCommand(new WacsdkPublisherCreateCommand(config, repoOption));
            AddCommand(new WacsdkPublisherListCommand(config, repoOption));

            // Add entity property management commands
            AddCommand(new EntityNameCommand(config, repoOption, publisherIdOption, valueOption));
            AddCommand(new EntityDescription(config, repoOption, publisherIdOption, valueOption));
            AddCommand(new EntityExtendedDescription(config, repoOption, publisherIdOption, valueOption));
            AddCommand(new EntityIsUnlistedCommand(config, repoOption, publisherIdOption, boolValueOption));
            AddCommand(new EntityForgetMeCommand(config, repoOption, publisherIdOption, nullableBoolValueOption));
            
            // Add collection management commands
            AddCommand(new ConnectionsCommand(config, repoOption, publisherIdOption, connectionIdOption, connectionValueOption));
            AddCommand(new EntityImagesCommand(config, repoOption, publisherIdOption, imagePathOption, requiredImageIdOption, optionalImageIdOption, imageNameOption));
            AddCommand(new LinksCommand(config, repoOption, publisherIdOption, linkIdOption, nameOption, urlOption, descriptionOption));
            AddCommand(new UserRolesCommand(config, repoOption, publisherIdOption, userIdOption, roleIdOption, roleNameOption, roleDescriptionOption));
            AddCommand(new AccentColorCommand(config, repoOption, publisherIdOption, colorOption));

            // Add publisher-specific collection commands
            AddCommand(new ProjectsCommand(config, repoOption, publisherIdOption, projectIdOption));
            AddCommand(new ChildPublishersCommand(config, repoOption, parentPublisherIdOption, childPublisherIdOption, roleIdOption, roleNameOption, roleDescriptionOption));
            AddCommand(new ParentPublishersCommand(config, repoOption, childPublisherIdOption, parentPublisherIdOption, roleIdOption, roleNameOption, roleDescriptionOption));
        }
    }
}
