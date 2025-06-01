using System.CommandLine;
using Ipfs.CoreApi;
using OwlCore.Diagnostics;
using OwlCore.Kubo;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Common.Images;

/// <summary>
/// Wacsdk get images command.
/// </summary>
public abstract class GetImagesCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetImagesCommand"/> class.
    /// </summary>
    public GetImagesCommand(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption)
        : base("get", $"Gets the images for the {entityType}.")
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

        Logger.LogInformation($"Enumerating images:");
        await foreach (var image in entity.GetImageFilesAsync(cancellationToken))
        {
            Logger.LogInformation($"- {nameof(image.Id)}: {image.Id}");
            Logger.LogInformation($"  {nameof(image.Name)}: {image.Name}");

            var cid = await image.GetCidAsync(Config.Client, new AddFileOptions { Pin = Config.KuboOptions.ShouldPin }, cancellationToken);
            Logger.LogInformation($"  {nameof(StorableKuboExtensions.GetCidAsync)}: {cid}");
            Logger.LogInformation($"  Type: {image.GetType()}");
        }
    }

    public abstract Task<IReadOnlyImagesCollection> GetEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);
}
