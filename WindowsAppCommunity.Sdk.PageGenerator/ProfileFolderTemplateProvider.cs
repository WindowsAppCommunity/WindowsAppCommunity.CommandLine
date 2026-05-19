using CommunityToolkit.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk.PageGenerator.Templating;

namespace WindowsAppCommunity.Sdk.PageGenerator;

public class ProfileFolderTemplateProvider<TEntity>(IFolder sourceFolder,
    ProfileFolderTemplateProvider<TEntity>.ProviderOverrideFactory? providerOverrideFactory = null)
    : IProfileTemplateProvider<TEntity> where TEntity : IReadOnlyEntity
{
    private readonly ProviderOverrideFactory _providerOverrideFactory = providerOverrideFactory
        ?? (_ => Task.FromResult<IProfileTemplateProvider<TEntity>?>(null));

    /// <inheritdoc/>
    public async Task ApplyTemplate(TEntity entity, IModifiableFolder outputFolder, CancellationToken token = default)
    {
        Guard.IsNotNull(entity);
        Guard.IsNotNull(outputFolder);

        // Recursively copy files from the source directory
        
    }
    
    public delegate Task<IProfileTemplateProvider<TEntity>?> ProviderOverrideFactory(IFile file);
}