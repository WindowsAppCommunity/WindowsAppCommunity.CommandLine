using System.CommandLine;
using OwlCore.Diagnostics;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Common.Projects;

/// <summary>
/// Wacsdk remove project command.
/// </summary>
public abstract class RemoveProjectCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveProjectCommand"/> class.
    /// </summary>
    public RemoveProjectCommand(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption, Option<string> projectIdOption)
        : base("remove", $"Removes a project from the {entityType}.")
    {
        AddOption(repoOption);
        AddOption(idOption);
        AddOption(projectIdOption);

        this.SetHandler(InvokeAsync, repoOption, idOption, projectIdOption);
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

        var projectToRemove = await entity.GetProjectsAsync(cancellationToken)
                                                .FirstOrDefaultAsync(x => x.Id == projectId, cancellationToken);
        if (projectToRemove is not null)
        {
            Logger.LogInformation($"Removing project");
            Logger.LogInformation($"Project");
            Logger.LogInformation($"    {nameof(projectToRemove.Id)}: {projectToRemove.Id}");
            Logger.LogInformation($"    {nameof(projectToRemove.Name)}: {projectToRemove.Name}");
            Logger.LogInformation($"    {nameof(projectToRemove.Description)}: {projectToRemove.Description}");

            await entity.RemoveProjectAsync(projectToRemove, cancellationToken);
            await PublishAsync(entity, cancellationToken);
        }
        else
        {
            Logger.LogWarning($"Project with ID {projectId} not found.");
        }
    }

    public abstract Task<IModifiableProjectCollection<IReadOnlyProject>> GetModifiableEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);

    public abstract Task PublishAsync(IModifiableProjectCollection<IReadOnlyProject> entity, CancellationToken cancellationToken);
}