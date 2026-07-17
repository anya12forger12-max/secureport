using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Provides read and write access to application configuration.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Gets the current application configuration.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The current application configuration.</returns>
    Task<AppConfiguration> GetConfigurationAsync(CancellationToken ct);

    /// <summary>
    /// Saves the application configuration.
    /// </summary>
    /// <param name="configuration">The configuration to save.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that completes when the configuration has been saved.</returns>
    Task SaveConfigurationAsync(AppConfiguration configuration, CancellationToken ct);

    /// <summary>
    /// Updates a specific section of the configuration.
    /// </summary>
    /// <param name="update">A function that receives the current configuration and returns the modified version.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that completes when the configuration has been updated.</returns>
    Task UpdateConfigurationAsync(Func<AppConfiguration, AppConfiguration> update, CancellationToken ct);

    /// <summary>
    /// Resets the configuration to its default values.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The default configuration that was applied.</returns>
    Task<AppConfiguration> ResetToDefaultsAsync(CancellationToken ct);

    /// <summary>
    /// Occurs when the configuration has been changed.
    /// </summary>
    event EventHandler<AppConfiguration>? ConfigurationChanged;
}
