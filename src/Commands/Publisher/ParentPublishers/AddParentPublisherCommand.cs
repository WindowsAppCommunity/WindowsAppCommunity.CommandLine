using System.CommandLine;
using CommunityToolkit.Diagnostics;
using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;
using WindowsAppCommunity.Sdk.Nomad;

namespace WindowsAppCommunity.CommandLine.Publisher.ParentPublishers;

/// <summary>
/// Wacsdk add parent publisher command for Publisher.
/// </summary>
public class AddParentPublisherCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddParentPublisherCommand"/> class.
    /// </summary>
    public AddParentPublisherCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> idOption, Option<string> publisherIdOption, Option<string> roleIdOption, Option<string> roleNameOption, Option<string> roleDescriptionOption)
        : base("add", "Adds a parent publisher to the Publisher.")
    {
        AddOption(repoOption);
        AddOption(idOption);
        AddOption(publisherIdOption);
        AddOption(roleIdOption);
        AddOption(roleNameOption);
        AddOption(roleDescriptionOption);

        this.SetHandler(InvokeAsync, repoOption, idOption, publisherIdOption, roleIdOption, roleNameOption, roleDescriptionOption);
        this.Config = config;
    }

    protected WacsdkCommandConfig Config { get; init; }

    public async Task InvokeAsync(string repo, string id, string publisherId, string roleId, string roleName, string roleDescription)
    {
        Guard.IsNotNullOrWhiteSpace(publisherId);

        var cancellationToken = Config.CancellationToken;
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Getting modifiable publisher");
        var publisher = (ModifiablePublisher)await GetPublisherByIdAsync(repo, id, cancellationToken);
        Logger.LogInformation($"Got {nameof(publisher.Id)}: {publisher.Id}");

        Logger.LogInformation($"Getting parent publisher");
        var childPublisher = await GetPublisherByIdAsync(repo, publisherId, cancellationToken);
        Logger.LogInformation($"Got {nameof(childPublisher.Id)}: {childPublisher.Id}");

        var role = new Role
        {
            Id = roleId,
            Name = roleName,
            Description = roleDescription
        };

        var readOnlyPublisher = childPublisher is ModifiablePublisher modifiablePublisher ? modifiablePublisher.InnerPublisher : (ReadOnlyPublisher)childPublisher;

        var publisherRole = new ReadOnlyPublisherRole { Role = role, InnerPublisher = readOnlyPublisher };

        Logger.LogInformation($"Adding parent publisher to collection");
        await publisher.ParentPublishers.AddPublisherAsync(publisherRole, cancellationToken);

        Logger.LogInformation($"Publishing changes");
        await PublishAsync(publisher, cancellationToken);

        Logger.LogInformation($"Parent publisher added successfully");
    }

    public async Task<IReadOnlyPublisher> GetPublisherByIdAsync(string repoId, string publisherId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var repoFolder = (IModifiableFolder)await Config.RepositoryStorage.CreateFolderAsync(repoId, overwrite: false, cancellationToken);
        var repoSettings = new WacsdkNomadSettings(repoFolder);
        await repoSettings.LoadAsync(cancellationToken);

        var repositoryContainer = new RepositoryContainer(Config.KuboOptions, Config.Client, repoSettings.ManagedKeys, repoSettings.ManagedUserConfigs, repoSettings.ManagedProjectConfigs, repoSettings.ManagedPublisherConfigs);
        var publisher = await repositoryContainer.PublisherRepository.GetAsync(publisherId, cancellationToken);

        return publisher;
    }

    public async Task PublishAsync(IModifiablePublisher publisher, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Logger.LogInformation($"Publishing changes made to {publisher.Id}");
        ModifiablePublisher modifiablePublisher = (ModifiablePublisher)publisher;
        await modifiablePublisher.FlushAsync(cancellationToken);
    }
}