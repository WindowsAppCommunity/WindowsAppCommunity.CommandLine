using System.CommandLine;
using OwlCore.Diagnostics;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Common.Images;

/// <summary>
/// Wacsdk remove image command.
/// </summary>
public abstract class RemoveImageCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveImageCommand"/> class.
    /// </summary>
    public RemoveImageCommand(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption, Option<string> imageIdOption)
        : base("remove", $"Removes an image from the {entityType}.")
    {
        AddOption(repoOption);
        AddOption(idOption);
        AddOption(imageIdOption);

        this.SetHandler(InvokeAsync, repoOption, idOption, imageIdOption);
        this.Config = config;
    }

    protected WacsdkCommandConfig Config { get; init; }

    public async Task InvokeAsync(string repo, string id, string imageId)
    {
        var cancellationToken = Config.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Getting modifiable entity");
        var entity = await GetModifiableEntityAsync(repo, id, cancellationToken);
        Logger.LogInformation($"Got {nameof(entity.Id)}: {entity.Id}");

        await entity.RemoveImageAsync(imageId, cancellationToken);
        Logger.LogInformation($"Removed image with id {imageId}");

        await PublishAsync(entity, cancellationToken);
    }

    public abstract Task<IModifiableImagesCollection> GetModifiableEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);

    public abstract Task PublishAsync(IModifiableImagesCollection entity, CancellationToken cancellationToken);
}
