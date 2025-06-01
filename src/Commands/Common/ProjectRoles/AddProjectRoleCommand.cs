using System.CommandLine;
using OwlCore.Diagnostics;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Common.ProjectRoles;

/// <summary>
/// Wacsdk add project role command.
/// </summary>
public abstract class AddProjectRoleCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddProjectRoleCommand"/> class.
    /// </summary>
    public AddProjectRoleCommand(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption, Option<string> projectIdOption, Option<string> roleIdOption, Option<string> roleNameOption, Option<string> roleDescriptionOption)
        : base("add", $"Adds a project role to the {entityType}.")
    {
        AddOption(repoOption);
        AddOption(idOption);
        AddOption(projectIdOption);
        AddOption(roleIdOption);
        AddOption(roleNameOption);
        AddOption(roleDescriptionOption);

        this.SetHandler(InvokeAsync, repoOption, idOption, projectIdOption, roleIdOption, roleNameOption, roleDescriptionOption);
        this.Config = config;
    }

    protected WacsdkCommandConfig Config { get; init; }

    public async Task InvokeAsync(string repo, string id, string projectId, string roleId, string roleName, string roleDescription)
    {
        var cancellationToken = Config.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Getting modifiable entity");
        var entity = await GetModifiableEntityAsync(repo, id, cancellationToken);
        Logger.LogInformation($"Got {nameof(entity.Id)}: {entity.Id}");

        Logger.LogInformation($"Getting project");
        var project = await GetProjectByIdAsync(repo, projectId, cancellationToken);
        Logger.LogInformation($"Got {nameof(project.Id)}: {project.Id}");

        var role = new Role
        {
            Id = roleId,
            Name = roleName,
            Description = roleDescription
        };

        var readOnlyProject = project is ModifiableProject modifiableProject ? modifiableProject.InnerProject : (ReadOnlyProject)project;

        var projectRole = new ReadOnlyProjectRole { Role = role, InnerProject = readOnlyProject };

        Logger.LogInformation($"Adding project");
        await entity.AddProjectAsync(projectRole, cancellationToken);

        await PublishAsync(entity, cancellationToken);
    }

    public abstract Task<IModifiableProjectRoleCollection> GetModifiableEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);

    public abstract Task<IReadOnlyProject> GetProjectByIdAsync(string repoId, string projectId, CancellationToken cancellationToken);

    public abstract Task PublishAsync(IModifiableProjectRoleCollection entity, CancellationToken cancellationToken);
}