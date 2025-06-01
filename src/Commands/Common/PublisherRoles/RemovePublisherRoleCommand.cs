using System.CommandLine;
using OwlCore.Diagnostics;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Common.PublisherRoles;

/// <summary>
/// Wacsdk remove publisher role command.
/// </summary>
public abstract class RemovePublisherRoleCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemovePublisherRoleCommand"/> class.
    /// </summary>
    public RemovePublisherRoleCommand(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption, Option<string> publisherRoleIdOption)
        : base("remove", $"Removes a publisher role from the {entityType}.")
    {
        AddOption(repoOption);
        AddOption(idOption);
        AddOption(publisherRoleIdOption);

        this.SetHandler(InvokeAsync, repoOption, idOption, publisherRoleIdOption);
        this.Config = config;
    }

    protected WacsdkCommandConfig Config { get; init; }

    public async Task InvokeAsync(string repoId, string id, string publisherId)
    {
        var cancellationToken = Config.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Getting modifiable entity");
        var entity = await GetModifiableEntityAsync(repoId, id, cancellationToken);
        Logger.LogInformation($"Got {nameof(entity.Id)}: {entity.Id}");

        var publisherRoleToRemove = await entity.GetPublishersAsync(cancellationToken)
                                                .FirstOrDefaultAsync(x => x.Id == publisherId, cancellationToken);
        if (publisherRoleToRemove is not null)
        {
            Logger.LogInformation($"Removing publisher role");
            Logger.LogInformation($"Publisher");
            Logger.LogInformation($"    {nameof(publisherRoleToRemove.Id)}: {publisherRoleToRemove.Id}");
            Logger.LogInformation($"    {nameof(publisherRoleToRemove.Name)}: {publisherRoleToRemove.Name}");
            Logger.LogInformation($"    {nameof(publisherRoleToRemove.Description)}: {publisherRoleToRemove.Description}");

            Logger.LogInformation($"    Role");
            Logger.LogInformation($"      {nameof(publisherRoleToRemove.Role.Id)}: {publisherRoleToRemove.Role.Id}");
            Logger.LogInformation($"      {nameof(publisherRoleToRemove.Role.Name)}: {publisherRoleToRemove.Role.Name}");
            Logger.LogInformation($"      {nameof(publisherRoleToRemove.Role.Description)}: {publisherRoleToRemove.Role.Description}");

            await entity.RemovePublisherAsync(publisherRoleToRemove, cancellationToken);
            await PublishAsync(entity, cancellationToken);
        }
        else
        {
            Logger.LogWarning($"Publisher role with ID {publisherId} not found.");
        }
    }

    public abstract Task<IModifiablePublisherRoleCollection> GetModifiableEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);

    public abstract Task<IReadOnlyPublisher> GetPublisherByIdAsync(string repoId, string publisherId, CancellationToken cancellationToken);

    public abstract Task PublishAsync(IModifiablePublisherRoleCollection entity, CancellationToken cancellationToken);
}