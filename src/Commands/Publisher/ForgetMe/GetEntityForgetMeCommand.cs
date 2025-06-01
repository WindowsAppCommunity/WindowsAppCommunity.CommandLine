using System.CommandLine;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Publisher.ForgetMe;

/// <summary>
/// Wacsdk get forget me command for Publisher.
/// </summary>
public class GetEntityForgetMeCommand : Common.Entity.ForgetMe.GetEntityForgetMeCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetEntityForgetMeCommand"/> class.
    /// </summary>
    public GetEntityForgetMeCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption)
        : base(config, "Publisher", repoOption, idOption)
    {
    }

    public override async Task<IReadOnlyEntity> GetEntityAsync(string repoId, string publisherId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var publisher = await repositoryContainer.PublisherRepository.GetAsync(publisherId, cancellationToken);

        return publisher;
    }
}