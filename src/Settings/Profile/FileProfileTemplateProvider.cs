using OwlCore.Storage;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Settings.Profile;

/// <summary>
/// A file-based profile template provider. Defers opening the template file until the template needs to be applied.
/// </summary>
public class FileProfileTemplateProvider<TEntity> : IProfileTemplateProvider<TEntity>
    where TEntity : IReadOnlyEntity
{
    private ProviderFactoryDelegate _providerFactory;

    /// <summary>
    /// Creates a new profile template provider sourced from the given file.
    /// </summary>
    /// <param name="templateFile">The file containing the desired template.</param>
    /// <param name="providerFactory">A method to create the inner template provider from the given file.</param>
    public FileProfileTemplateProvider(IFile templateFile, ProviderFactoryDelegate providerFactory)
    {
        TemplateFile = templateFile;
        _providerFactory = providerFactory;
    }

    /// <summary>
    /// The file containing the template.
    /// </summary>
    public IFile TemplateFile { get; init; }

    /// <inheritdoc/>
    public async Task ApplyTemplate(TEntity entity, IModifiableFolder outputFolder, CancellationToken token = default)
    {
        var template = await _providerFactory(TemplateFile, token);
        await template.ApplyTemplate(entity, outputFolder, token);
    }

    /// <summary>
    /// Creates a template provider from the given file.
    /// </summary>
    public delegate Task<IProfileTemplateProvider<TEntity>> ProviderFactoryDelegate(IFile templateFile, CancellationToken token);
}
