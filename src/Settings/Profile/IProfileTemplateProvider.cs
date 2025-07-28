using OwlCore.Storage;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Settings.Profile;

/// <summary>
/// An interface for generating multiple static content to from an <see cref="IReadOnlyEntity"/>.
/// </summary>
public interface IProfileTemplateProvider<in TEntity> where TEntity : IReadOnlyEntity
{
    /// <summary>
    /// Generates static content from the data in <paramref name="entity"/>.
    /// </summary>
    /// <param name="entity">The entity to pull information from.</param>
    /// <param name="outputFolder">The folder to write the generated content to.</param>
    /// <param name="token">A token to cancel the operation.</param>
    /// <returns>A task that represents the operation.</returns>
    Task ApplyTemplate(TEntity entity, IModifiableFolder outputFolder, CancellationToken token = default);
}
