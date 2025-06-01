using System.CommandLine;
using OwlCore.Diagnostics;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Common.Projects;

/// <summary>
/// Wacsdk add project command.
/// </summary>
public abstract class AddProjectCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddProjectCommand"/> class.
    /// </summary>
    public AddProjectCommand(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption, Option<string> projectIdOption)
        : base("add", $"Adds a project to the {entityType}.")
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

        Logger.LogInformation($"Getting project to add");
        var project = await GetProjectByIdAsync(repoId, projectId, cancellationToken);
        Logger.LogInformation($"Got project {nameof(project.Id)}: {project.Id}");
        Logger.LogInformation($"Project {nameof(project.Name)}: {project.Name}");
        Logger.LogInformation($"Project {nameof(project.Description)}: {project.Description}");

        Logger.LogInformation($"Adding project to entity");
        await entity.AddProjectAsync(project, cancellationToken);
        await PublishAsync(entity, cancellationToken);
    }

    public abstract Task<IModifiableProjectCollection<IReadOnlyProject>> GetModifiableEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);

    public abstract Task<IReadOnlyProject> GetProjectByIdAsync(string repoId, string projectId, CancellationToken cancellationToken);

    public abstract Task PublishAsync(IModifiableProjectCollection<IReadOnlyProject> entity, CancellationToken cancellationToken);
}