using System.CommandLine;
using OwlCore.Diagnostics;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Common.Projects;

/// <summary>
/// Wacsdk get projects command.
/// </summary>
public abstract class GetProjectsCommands : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetProjectsCommands"/> class.
    /// </summary>
    public GetProjectsCommands(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption)
        : base("get", $"Gets the projects for the {entityType}.")
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
        await foreach (var project in entity.GetProjectsAsync(cancellationToken))
        {
            Logger.LogInformation($"Project:");
            Logger.LogInformation($"  Id: {project.Id}");
            Logger.LogInformation($"  Name: {project.Name}");
            Logger.LogInformation($"  Description: {project.Description}");
        }
    }

    public abstract Task<IReadOnlyProjectCollection<IReadOnlyProject>> GetEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);
}