using System.CommandLine;
using OwlCore.Diagnostics;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Common.ProjectRoles;

/// <summary>
/// Wacsdk get project roles command.
/// </summary>
public abstract class GetProjectRolesCommands : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetProjectRolesCommands"/> class.
    /// </summary>
    public GetProjectRolesCommands(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption)
        : base("get", $"Gets the project roles for the {entityType}.")
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

        Logger.LogInformation($"Enumerating projects");
        await foreach (var projectRole in entity.GetProjectsAsync(cancellationToken))
        {
            Logger.LogInformation($"Project");
            Logger.LogInformation($"    {nameof(projectRole.Id)}: {projectRole.Id}");
            Logger.LogInformation($"    {nameof(projectRole.Name)}: {projectRole.Name}");
            Logger.LogInformation($"    {nameof(projectRole.Description)}: {projectRole.Description}");

            Logger.LogInformation($"    Role");
            Logger.LogInformation($"      {nameof(projectRole.Role.Id)}: {projectRole.Role.Id}");
            Logger.LogInformation($"      {nameof(projectRole.Role.Name)}: {projectRole.Role.Name}");
            Logger.LogInformation($"      {nameof(projectRole.Role.Description)}: {projectRole.Role.Description}");
        }
    }

    public abstract Task<IReadOnlyProjectRoleCollection> GetEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);
}