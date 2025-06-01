using System.CommandLine;
using OwlCore.Diagnostics;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Common.PublisherRoles;

/// <summary>
/// Wacsdk add publisher role command.
/// </summary>
public abstract class AddPublisherRoleCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddPublisherRoleCommand"/> class.
    /// </summary>
    public AddPublisherRoleCommand(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption, Option<string> publisherIdOption, Option<string> roleIdOption, Option<string> roleNameOption, Option<string> roleDescriptionOption)
        : base("add", $"Adds a publisher role to the {entityType}.")
    {
        AddOption(repoOption);
        AddOption(idOption);
        AddOption(publisherIdOption);
        AddOption(roleIdOption);
        AddOption(roleNameOption);
        AddOption(roleDescriptionOption);

        this.SetHandler(InvokeAsync, repoOption, idOption, publisherIdOption, roleIdOption, roleNameOption, roleDescriptionOption);
        this.Config = config;
    }

    protected WacsdkCommandConfig Config { get; init; }

    public async Task InvokeAsync(string repo, string id, string publisherId, string roleId, string roleName, string roleDescription)
    {
        var cancellationToken = Config.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Getting modifiable entity");
        var entity = await GetModifiableEntityAsync(repo, id, cancellationToken);
        Logger.LogInformation($"Got {nameof(entity.Id)}: {entity.Id}");

        Logger.LogInformation($"Getting publisher");
        var publisher = await GetPublisherByIdAsync(repo, publisherId, cancellationToken);
        Logger.LogInformation($"Got {nameof(publisher.Id)}: {publisher.Id}");

        var role = new Role
        {
            Id = roleId,
            Name = roleName,
            Description = roleDescription
        };

        var readOnlyPublisher = publisher is ModifiablePublisher modifiablePublisher ? modifiablePublisher.InnerPublisher : (ReadOnlyPublisher)publisher;

        var publisherRole = new ReadOnlyPublisherRole { Role = role, InnerPublisher = readOnlyPublisher };

        Logger.LogInformation($"Adding publisher");
        await entity.AddPublisherAsync(publisherRole, cancellationToken);

        await PublishAsync(entity, cancellationToken);
    }

    public abstract Task<IModifiablePublisherRoleCollection> GetModifiableEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);

    public abstract Task<IReadOnlyPublisher> GetPublisherByIdAsync(string repoId, string publisherId, CancellationToken cancellationToken);

    public abstract Task PublishAsync(IModifiablePublisherRoleCollection entity, CancellationToken cancellationToken);
}