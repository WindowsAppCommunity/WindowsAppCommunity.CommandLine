using System.CommandLine;
using OwlCore.Diagnostics;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Common.ProjectRoles;

/// <summary>
/// Wacsdk remove project role command.
/// </summary>
public abstract class RemoveProjectRoleCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveProjectRoleCommand"/> class.
    /// </summary>
    public RemoveProjectRoleCommand(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption, Option<string> projectRoleIdOption)
        : base("remove", $"Removes a project role from the {entityType}.")
    {
        AddOption(repoOption);
        AddOption(idOption);
        AddOption(projectRoleIdOption);

        this.SetHandler(InvokeAsync, repoOption, idOption, projectRoleIdOption);
        this.Config = config;
    }

    protected WacsdkCommandConfig Config { get; init; }

    public async Task InvokeAsync(string repoId, string id, string projectId)
    {
        var cancellationToken = Config.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Getting modifiable entity");
        var entity = await GetModifiableEntityAsync(repoId, id, cancellationToken);
        Logger.LogInformation($"Got {nameof(entity.Id)}: {entity.Id}");

        var projectRoleToRemove = await entity.GetProjectsAsync(cancellationToken)
                                                .FirstOrDefaultAsync(x => x.Id == projectId, cancellationToken);
        if (projectRoleToRemove is not null)
        {
            Logger.LogInformation($"Removing project role");
            Logger.LogInformation($"Project");
            Logger.LogInformation($"    {nameof(projectRoleToRemove.Id)}: {projectRoleToRemove.Id}");
            Logger.LogInformation($"    {nameof(projectRoleToRemove.Name)}: {projectRoleToRemove.Name}");
            Logger.LogInformation($"    {nameof(projectRoleToRemove.Description)}: {projectRoleToRemove.Description}");

            Logger.LogInformation($"    Role");
            Logger.LogInformation($"      {nameof(projectRoleToRemove.Role.Id)}: {projectRoleToRemove.Role.Id}");
            Logger.LogInformation($"      {nameof(projectRoleToRemove.Role.Name)}: {projectRoleToRemove.Role.Name}");
            Logger.LogInformation($"      {nameof(projectRoleToRemove.Role.Description)}: {projectRoleToRemove.Role.Description}");

            await entity.RemoveProjectAsync(projectRoleToRemove, cancellationToken);
            await PublishAsync(entity, cancellationToken);
        }
        else
        {
            Logger.LogWarning($"Project role with ID {projectId} not found.");
        }
    }

    public abstract Task<IModifiableProjectRoleCollection> GetModifiableEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);

    public abstract Task<IReadOnlyProject> GetProjectByIdAsync(string repoId, string projectId, CancellationToken cancellationToken);

    public abstract Task PublishAsync(IModifiableProjectRoleCollection entity, CancellationToken cancellationToken);
}