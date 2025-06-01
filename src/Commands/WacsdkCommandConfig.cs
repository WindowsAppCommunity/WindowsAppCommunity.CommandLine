using Ipfs.CoreApi;
using OwlCore.Nomad.Kubo;
using OwlCore.Storage;

namespace WindowsAppCommunity.CommandLine;

public class WacsdkCommandConfig
{
    /// <summary>
    /// Gets or initializes the repository storage.
    /// </summary>
    public required IModifiableFolder RepositoryStorage { get; init; }

    /// <summary>
    /// Gets or initializes the IPFS Core API client.
    /// </summary>
    public required ICoreApi Client { get; init; }

    /// <summary>
    /// Gets or initializes the Kubo options.
    /// </summary>
    public required IKuboOptions KuboOptions { get; init; }

    /// <summary>
    /// A token that can be used to cancel the operation.
    /// </summary>
    public required CancellationToken CancellationToken { get; init; }
}