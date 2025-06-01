using System.CommandLine;
using CommunityToolkit.Diagnostics;
using OwlCore.Diagnostics;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Common.UserRoles;

/// <summary>
/// Wacsdk add user role command.
/// </summary>
public abstract class AddUserRoleCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddUserRoleCommand"/> class.
    /// </summary>
    public AddUserRoleCommand(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption, Option<string> userIdOption, Option<string> roleIdOption, Option<string> roleNameOption, Option<string> roleDescriptionOption)
        : base("add", $"Adds a user role to the {entityType}.")
    {
        AddOption(repoOption);
        AddOption(idOption);
        AddOption(userIdOption);
        AddOption(roleIdOption);
        AddOption(roleNameOption);
        AddOption(roleDescriptionOption);

        this.SetHandler(InvokeAsync, repoOption, idOption, userIdOption, roleIdOption, roleNameOption, roleDescriptionOption);
        this.Config = config;
    }

    protected WacsdkCommandConfig Config { get; init; }

    public async Task InvokeAsync(string repo, string id, string userId, string roleId, string roleName, string roleDescription)
    {
        Guard.IsNotNullOrWhiteSpace(userId);
        Guard.IsNotNullOrWhiteSpace(roleId);
        Guard.IsNotNullOrWhiteSpace(roleName);

        var cancellationToken = Config.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Getting modifiable entity");
        var entity = await GetModifiableEntityAsync(repo, id, cancellationToken);
        Logger.LogInformation($"Got {nameof(entity.Id)}: {entity.Id}");

        Logger.LogInformation($"Getting user");
        var user = await GetUserByIdAsync(repo, userId, cancellationToken);
        Logger.LogInformation($"Got {nameof(user.Id)}: {user.Id}");

        var role = new Role
        {
            Id = roleId,
            Name = roleName,
            Description = roleDescription
        };

        var readOnlyUser = user is ModifiableUser modifiableUser ? modifiableUser.InnerUser : (ReadOnlyUser)user;

        var userRole = new ReadOnlyUserRole { Role = role, InnerUser = readOnlyUser };

        Logger.LogInformation($"Adding user");
        await entity.AddUserAsync(userRole, cancellationToken);

        await PublishAsync(entity, cancellationToken);
    }

    public abstract Task<IModifiableUserRoleCollection> GetModifiableEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);

    public abstract Task<IReadOnlyUser> GetUserByIdAsync(string repoId, string userId, CancellationToken cancellationToken);

    public abstract Task PublishAsync(IModifiableUserRoleCollection entity, CancellationToken cancellationToken);
}