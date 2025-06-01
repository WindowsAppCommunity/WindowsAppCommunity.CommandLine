using System.CommandLine;
using OwlCore.Diagnostics;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Common.UserRoles;

/// <summary>
/// Wacsdk get user roles command.
/// </summary>
public abstract class GetUserRolesCommands : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserRolesCommands"/> class.
    /// </summary>
    public GetUserRolesCommands(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption)
        : base("get", $"Gets the user roles for the {entityType}.")
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

        Logger.LogInformation($"Enumerating users");
        await foreach (var userRole in entity.GetUsersAsync(cancellationToken))
        {
            Logger.LogInformation($"User");
            Logger.LogInformation($"    {nameof(userRole.Id)}: {userRole.Id}");
            Logger.LogInformation($"    {nameof(userRole.Name)}: {userRole.Name}");
            Logger.LogInformation($"    {nameof(userRole.Description)}: {userRole.Description}");

            Logger.LogInformation($"    Role");
            Logger.LogInformation($"      {nameof(userRole.Role.Id)}: {userRole.Role.Id}");
            Logger.LogInformation($"      {nameof(userRole.Role.Name)}: {userRole.Role.Name}");
            Logger.LogInformation($"      {nameof(userRole.Role.Description)}: {userRole.Role.Description}");
        }
    }

    public abstract Task<IReadOnlyUserRoleCollection> GetEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);
}