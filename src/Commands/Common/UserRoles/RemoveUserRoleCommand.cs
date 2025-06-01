using System.CommandLine;
using OwlCore.Diagnostics;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Common.UserRoles;

/// <summary>
/// Wacsdk remove user role command.
/// </summary>
public abstract class RemoveUserRoleCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveUserRoleCommand"/> class.
    /// </summary>
    public RemoveUserRoleCommand(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption, Option<string> userIdOption)
        : base("remove", $"Removes a user role from the {entityType}.")
    {
        AddOption(repoOption);
        AddOption(idOption);
        AddOption(userIdOption);

        this.SetHandler(InvokeAsync, repoOption, idOption, userIdOption);
        this.Config = config;
    }

    protected WacsdkCommandConfig Config { get; init; }

    public async Task InvokeAsync(string repoId, string id, string userId)
    {
        var cancellationToken = Config.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Getting modifiable entity");
        var entity = await GetModifiableEntityAsync(repoId, id, cancellationToken);
        Logger.LogInformation($"Got {nameof(entity.Id)}: {entity.Id}");

        var userRoleToRemove = await entity.GetUsersAsync(cancellationToken)
                                                .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (userRoleToRemove is not null)
        {
            Logger.LogInformation($"Removing user role");
            Logger.LogInformation($"User");
            Logger.LogInformation($"    {nameof(userRoleToRemove.Id)}: {userRoleToRemove.Id}");
            Logger.LogInformation($"    {nameof(userRoleToRemove.Name)}: {userRoleToRemove.Name}");
            Logger.LogInformation($"    {nameof(userRoleToRemove.Description)}: {userRoleToRemove.Description}");

            Logger.LogInformation($"    Role");
            Logger.LogInformation($"      {nameof(userRoleToRemove.Role.Id)}: {userRoleToRemove.Role.Id}");
            Logger.LogInformation($"      {nameof(userRoleToRemove.Role.Name)}: {userRoleToRemove.Role.Name}");
            Logger.LogInformation($"      {nameof(userRoleToRemove.Role.Description)}: {userRoleToRemove.Role.Description}");

            await entity.RemoveUserAsync(userRoleToRemove, cancellationToken);
            await PublishAsync(entity, cancellationToken);
        }
        else
        {
            Logger.LogWarning($"User role with ID {userId} not found.");
        }
    }

    public abstract Task<IModifiableUserRoleCollection> GetModifiableEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);

    public abstract Task<IReadOnlyUser> GetUserByIdAsync(string repoId, string userId, CancellationToken cancellationToken);

    public abstract Task PublishAsync(IModifiableUserRoleCollection entity, CancellationToken cancellationToken);
}