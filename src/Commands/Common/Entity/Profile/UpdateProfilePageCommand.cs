using OwlCore.Diagnostics;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using System.CommandLine;
using WindowsAppCommunity.CommandLine.Settings.Profile;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Common.Profile;

/// <summary>
/// Wacsdk update profile page command.
/// </summary>
public abstract class UpdateProfilePageCommand<TEntity> : Command
    where TEntity : IReadOnlyEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateProfilePageCommand{TEntity}"/> class.
    /// </summary>
    public UpdateProfilePageCommand(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption, Option<string> templatePathOption, Option<string> outputFileNameOption)
        : base("page", $"Regenerates the static profile page for {entityType}.")
    {
        AddOption(repoOption);
        AddOption(idOption);
        AddOption(templatePathOption);
        AddOption(outputFileNameOption);

        this.SetHandler(InvokeAsync, repoOption, idOption, templatePathOption, outputFileNameOption);
        this.Config = config;
    }

    protected WacsdkCommandConfig Config { get; init; }

    /// <inheritdoc/>
    public async Task InvokeAsync(string repoId, string entityId, string templatePath, string outputFileName)
    {
        var cancellationToken = Config.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Getting entity");
        var entity = await GetEntityAsync(repoId, entityId, cancellationToken);
        Logger.LogInformation($"Got {nameof(entity.Id)}: {entity.Id}");

        var thisRepoStorage = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false);
        var thisEntityFolder = (IModifiableFolder)await thisRepoStorage.CreateFolderAsync(entityId);
        var outputFolder = (IModifiableFolder)await thisEntityFolder.CreateFolderAsync("profile");
        Logger.LogInformation($"Generating profile page in {outputFolder.Id}");

        var templateFile = new SystemFile(templatePath);
        var template = await ScribanProfileTemplateProvider<TEntity>.CreateFromFileAsync(templateFile, outputFileName, cancellationToken);
        await template.ApplyTemplate(entity, outputFolder, cancellationToken);
    }

    public abstract Task<TEntity> GetEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);
}
