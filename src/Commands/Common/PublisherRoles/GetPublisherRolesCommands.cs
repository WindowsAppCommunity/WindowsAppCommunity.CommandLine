using System.CommandLine;
using OwlCore.Diagnostics;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Common.PublisherRoles;

/// <summary>
/// Wacsdk get publisher roles command.
/// </summary>
public abstract class GetPublisherRolesCommands : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetPublisherRolesCommands"/> class.
    /// </summary>
    public GetPublisherRolesCommands(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption)
        : base("get", $"Gets the publisher roles for the {entityType}.")
    {
        AddOption(repoOption);
        AddOption(idOption);

        this.SetHandler(InvokeAsync, repoOption, idOption);
        this.Config = config;
    }

    protected WacsdkCommandConfig Config { get; init; }

    public async Task InvokeAsync(string repo, string id)
    {
        var cancellationToken = Config.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();
        
        Logger.LogInformation($"Getting entity");
        var entity = await GetEntityAsync(repo, id, cancellationToken);
        Logger.LogInformation($"Got {nameof(entity.Id)}: {entity.Id}");

        Logger.LogInformation($"Enumerating publishers");
        await foreach (var publisherRole in entity.GetPublishersAsync(cancellationToken))
        {
            Logger.LogInformation($"Publisher");
            Logger.LogInformation($"    {nameof(publisherRole.Id)}: {publisherRole.Id}");
            Logger.LogInformation($"    {nameof(publisherRole.Name)}: {publisherRole.Name}");
            Logger.LogInformation($"    {nameof(publisherRole.Description)}: {publisherRole.Description}");

            Logger.LogInformation($"    Role");
            Logger.LogInformation($"      {nameof(publisherRole.Role.Id)}: {publisherRole.Role.Id}");
            Logger.LogInformation($"      {nameof(publisherRole.Role.Name)}: {publisherRole.Role.Name}");
            Logger.LogInformation($"      {nameof(publisherRole.Role.Description)}: {publisherRole.Role.Description}");
        }
    }

    public abstract Task<IReadOnlyPublisherRoleCollection> GetEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);
}