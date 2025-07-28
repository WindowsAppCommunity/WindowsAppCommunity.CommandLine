using OwlCore.Diagnostics;
using OwlCore.Storage;
using System.CommandLine;
using WindowsAppCommunity.CommandLine.Settings.Profile;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Common.Profile;

/// <summary>
/// Wacsdk add connection command.
/// </summary>
public abstract class UpdateProfilePageCommand<TEntity> : Command
    where TEntity : IReadOnlyEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateProfilePageCommand{TEntity}"/> class.
    /// </summary>
    public UpdateProfilePageCommand(WacsdkCommandConfig config, IProfileTemplateProvider<TEntity> template, string entityType, Option<string> repoOption, Option<string> idOption)
        : base("page", $"Regenerates the static profile page for {entityType}.")
    {
        AddOption(repoOption);
        AddOption(idOption);

        this.SetHandler(InvokeAsync, repoOption, idOption);
        this.Config = config;
        this.Template = template;
    }

    protected WacsdkCommandConfig Config { get; init; }

    public IProfileTemplateProvider<TEntity> Template { get; init; }

    public async Task InvokeAsync(string repoId, string entityId)
    {
        var cancellationToken = Config.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Getting entity");
        var entity = await GetEntityAsync(repoId, entityId, cancellationToken);
        Logger.LogInformation($"Got {nameof(entity.Id)}: {entity.Id}");

        var thisRepoStorage = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false);
        var thisEntityFolder = (IModifiableFolder)await thisRepoStorage.CreateFolderAsync(entityId);
        var outputFolder = (IModifiableFolder)await thisEntityFolder.CreateFolderAsync("profile");

        await Template.ApplyTemplate(entity, outputFolder, cancellationToken);
    }

    public abstract Task<TEntity> GetEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);
}
