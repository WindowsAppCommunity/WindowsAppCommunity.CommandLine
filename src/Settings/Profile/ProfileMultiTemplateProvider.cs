using CommunityToolkit.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Settings.Profile;

/// <summary>
/// Applies multiple templates to the same entity and outputs the results to the same folder.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <remarks>Can be used to template HTML and CSS for a webpage.</remarks>
public class ProfileMultiTemplateProvider<TEntity> : IProfileTemplateProvider<TEntity>
    where TEntity : IReadOnlyEntity
{
    /// <summary>
    /// The templates to apply to an entity.
    /// </summary>
    public List<IProfileTemplateProvider<TEntity>> Templates { get; init; } = [];

    /// <inheritdoc/>
    public async Task ApplyTemplate(TEntity entity, IModifiableFolder outputFolder, CancellationToken token = default)
    {
        Guard.IsNotNull(entity);
        Guard.IsNotNull(outputFolder);

        foreach (var template in Templates)
        {
            await template.ApplyTemplate(entity, outputFolder, token);

            token.ThrowIfCancellationRequested();
        }
    }
}
