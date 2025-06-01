using System.CommandLine;
using Ipfs;
using OwlCore.Diagnostics;
using OwlCore.Kubo;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Common.Images;

/// <summary>
/// Wacsdk add image command.
/// </summary>
public abstract class AddImageCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddImageCommand"/> class.
    /// </summary>
    public AddImageCommand(WacsdkCommandConfig config, string entityType, Option<string> repoOption, Option<string> idOption, Option<string> imagePathOption, Option<string?> imageIdOption, Option<string?> imageNameOption)
        : base("add", $"Adds an image to the {entityType}.")
    {
        AddOption(repoOption);
        AddOption(idOption);
        AddOption(imagePathOption);
        AddOption(imageIdOption);
        AddOption(imageNameOption);

        this.SetHandler(InvokeAsync, repoOption, idOption, imagePathOption, imageIdOption, imageNameOption);
        this.Config = config;
    }

    protected WacsdkCommandConfig Config { get; init; }

    public async Task InvokeAsync(string repo, string id, string imagePath, string? imageId, string? imageName)
    {
        var cancellationToken = Config.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Getting modifiable entity");
        var entity = await GetModifiableEntityAsync(repo, id, cancellationToken);
        Logger.LogInformation($"Got {nameof(entity.Id)}: {entity.Id}");

        // Load the image file
        IFile imageFile = imagePath switch
        {
            _ when PathHelpers.IpfsProtocolPathValues.Any(imagePath.StartsWith) => new IpfsFile(cid: PathHelpers.RemoveProtocols(PathHelpers.RemoveQueries(imagePath), PathHelpers.IpfsProtocolPathValues), name: imageName ?? PathHelpers.RemoveProtocols(PathHelpers.RemoveQueries(imagePath), PathHelpers.IpfsProtocolPathValues), Config.Client),
            _ when PathHelpers.IpnsProtocolPathValues.Any(imagePath.StartsWith) => new IpnsFile(ipnsAddress: PathHelpers.RemoveProtocols(PathHelpers.RemoveQueries(imagePath), PathHelpers.IpnsProtocolPathValues), name: imageName ?? PathHelpers.TryGetFileNameFromPathQuery(PathHelpers.RemoveProtocols(imagePath, PathHelpers.IpnsProtocolPathValues)) ?? PathHelpers.GetFolderItemName(PathHelpers.RemoveProtocols(imagePath, PathHelpers.IpnsProtocolPathValues)), Config.Client),
            _ when PathHelpers.MfsProtocolPathValues.Any(imagePath.StartsWith) => new MfsFile(path: PathHelpers.RemoveProtocols(PathHelpers.RemoveQueries(imagePath), PathHelpers.MfsProtocolPathValues), Config.Client),
            _ when File.Exists(imagePath) => new ContentAddressedSystemFile(imagePath, Config.Client),
            _ => throw new ArgumentException($"Invalid image path: {imagePath}"),
        };

        Logger.LogInformation($"Input file:");
        Logger.LogInformation($"- {nameof(imageFile.Id)}: {imageFile.Id}");
        Logger.LogInformation($"  {nameof(imageFile.Name)}: {imageFile.Name}");
        Logger.LogInformation($"  {nameof(imageFile.GetType)}: {imageFile.GetType()}");

        await entity.AddImageAsync(imageFile, imageId ?? imageFile.Id, imageName, cancellationToken);
        var addedImage = await entity.GetImageFilesAsync(cancellationToken).FirstAsync(x=> x.Id == imageId || x.Name == imageName, cancellationToken: cancellationToken);
        Logger.LogInformation($"Added file:");
        Logger.LogInformation($"- {nameof(addedImage.Id)}: {addedImage.Id}");
        Logger.LogInformation($"  {nameof(addedImage.Name)}: {addedImage.Name}");
        Logger.LogInformation($"  {nameof(addedImage.GetType)}: {addedImage.GetType()}");

        await PublishAsync(entity, cancellationToken);
    }

    public abstract Task<IModifiableImagesCollection> GetModifiableEntityAsync(string repoId, string entityId, CancellationToken cancellationToken);

    public abstract Task PublishAsync(IModifiableImagesCollection entity, CancellationToken cancellationToken);
}
