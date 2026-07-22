namespace WindowsAppCommunity.Blog.Assets;

/// <summary>
/// The strategy to use when encountering an unknown asset.
/// </summary>
[Flags]
public enum FaultStrategy
{
    /// <summary>
    /// Nothing happens when an unknown asset it encountered. It is skipped without error or log.
    /// </summary>
    None,

    /// <summary>
    /// Logs a warning if an unknown asset is encountered.
    /// </summary>
    LogWarn,

    /// <summary>
    /// Logs an error without throwing if an unknown asset is encountered.
    /// </summary>
    LogError,

    /// <summary>
    /// Throws if an unknown asset is encountered.
    /// </summary>
    Throw,
}
