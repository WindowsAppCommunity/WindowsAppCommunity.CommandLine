using OwlCore.Storage;

namespace WindowsAppCommunity.Blog.Assets
{
    /// <summary>
    /// Captures complete asset reference information for materialization.
    /// Stores original detected path, rewritten path after strategy, and resolved file instance.
    /// </summary>
    /// <param name="OriginalPath">Path detected in markdown (relative to source file)</param>
    /// <param name="RewrittenPath">Path after inclusion strategy applied (include vs reference)</param>
    /// <param name="ResolvedFile">Actual file instance for copy operations</param>
    public record PageAsset(string OriginalPath, string RewrittenPath, IFile ResolvedFile);
}
