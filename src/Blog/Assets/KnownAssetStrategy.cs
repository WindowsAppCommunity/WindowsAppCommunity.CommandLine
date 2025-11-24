using OwlCore.Diagnostics;
using OwlCore.Storage;

namespace WindowsAppCommunity.Blog.Assets;

/// <summary>
/// Determines fallback asset behavior when the asset is not known to the strategy selector.
/// </summary>
public enum AssetFallbackBehavior
{
    /// <summary>
    /// The asset path is rewritten to support being referenced by the folderized markdown.
    /// </summary>
    Reference,

    /// <summary>
    /// The asset path is not rewritten and it is included in the output path.
    /// </summary>
    Include,

    /// <summary>
    /// The new asset path is returned as null and the asset is not included in the output.
    /// </summary>
    Drop,
}

/// <summary>
/// Uses a known list of files to decide between asset inclusion (child path) vs asset reference (parented path).
/// </summary>
public sealed class KnownAssetStrategy : IAssetStrategy
{
    /// <summary>
    /// A list of known file IDs to rewrite to an included asset.
    /// </summary>
    public HashSet<string> IncludedAssetFileIds { get; set; } = new();

    /// <summary>
    /// A list of known file IDs rewrite as a referenced asset.
    /// </summary>
    public HashSet<string> ReferencedAssetFileIds { get; set; } = new();

    /// <summary>
    /// The strategy to use when encountering an unknown asset.
    /// </summary> 
    public FaultStrategy UnknownAssetFaultStrategy { get; set; }

    /// <summary>
    /// Gets or sets the fallback used when the asset is unknown but <see cref="UnknownAssetFaultStrategy"/> does not have <see cref="FaultStrategy.Throw"/>.
    /// </summary>
    public AssetFallbackBehavior UnknownAssetFallbackStrategy { get; set; }

    /// <inheritdoc/>
    public async Task<string?> DecideAsync(IFile referencingMarkdown, IFile referencedAsset, string originalPath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(originalPath))
            return originalPath;

        var isReferenced = ReferencedAssetFileIds.Contains(referencedAsset.Id);
        var isIncluded = IncludedAssetFileIds.Contains(referencedAsset.Id);

        if (isReferenced)
            return $"../{originalPath}";

        if (isIncluded)
            return originalPath;

        // Handle as unknown
        HandleUnknownAsset(referencedAsset);

        return UnknownAssetFallbackStrategy switch
        {
            AssetFallbackBehavior.Reference => $"../{originalPath}",
            AssetFallbackBehavior.Include => originalPath,
            AssetFallbackBehavior.Drop => null,
            _ => throw new ArgumentOutOfRangeException(nameof(UnknownAssetFallbackStrategy)),
        };
    }

    private void HandleUnknownAsset(IFile referencedAsset)
    {
        var faultMessage = $"Unknown asset encountered: {nameof(referencedAsset.Name)} {referencedAsset.Name}, {nameof(referencedAsset.Id)} {referencedAsset.Id}. Please add this ID to either {nameof(IncludedAssetFileIds)} or {nameof(ReferencedAssetFileIds)}.";

        if (UnknownAssetFaultStrategy.HasFlag(FaultStrategy.LogWarn))
            Logger.LogWarning(faultMessage);

        if (UnknownAssetFaultStrategy.HasFlag(FaultStrategy.LogError))
            Logger.LogError(faultMessage);

        if (UnknownAssetFaultStrategy.HasFlag(FaultStrategy.Throw))
            throw new InvalidOperationException(faultMessage);
    }
}
