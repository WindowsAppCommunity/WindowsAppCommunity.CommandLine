using CommunityToolkit.Diagnostics;
using OwlCore.Storage;

namespace WindowsAppCommunity.Sdk.PageGenerator.Templating;

public static class ProfileTemplateProvider
{
    /// <summary>
    /// Creates a template or multiple templates from the given storable.
    /// </summary>
    /// <typeparam name="TEntity">The entity supported by the template provider.</typeparam>
    /// <param name="storable">The storable to read templates from.</param>
    /// <param name="providerFactory">A function that creates the desired template provider from a file.</param>
    /// <param name="token">A token to cancel the operation.</param>
    /// <returns>A task that represents the operation.</returns>
    /// <exception cref="ArgumentException">A null or invalid storable was supplied.</exception>
    public static async Task<IProfileTemplateProvider<TEntity>> CreateFromStorable<TEntity>(IStorable storable, Func<IFile, IProfileTemplateProvider<TEntity>> providerFactory, CancellationToken token = default)
        where TEntity : IReadOnlyEntity
    {
        Guard.IsNotNull(storable);

        if (storable is IFolder folder)
        {
            List<IProfileTemplateProvider<TEntity>> templates = [];
            await foreach (var storableChild in folder.GetItemsAsync(cancellationToken: token))
            {
                var subtemplate = await CreateFromStorable(storableChild, providerFactory, token);
                templates.Add(subtemplate);
            }

            return new ProfileMultiTemplateProvider<TEntity>
            {
                Templates = templates,
            };
        }
        else if (storable is IFile file)
        {
            return providerFactory(file);
        }

        throw new ArgumentException($"Expected a file or folder, got '{storable.GetType().Name}'", nameof(storable));
    }
}
