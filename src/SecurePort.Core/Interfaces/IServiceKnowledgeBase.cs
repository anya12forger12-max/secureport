using SecurePort.Core.Models;

namespace SecurePort.Core.Interfaces;

/// <summary>
/// Maintains a local knowledge base of well-known network services.
/// </summary>
public interface IServiceKnowledgeBase
{
    /// <summary>
    /// Gets all knowledge base entries.
    /// </summary>
    IReadOnlyList<ServiceKnowledgeEntry> GetAllEntries();

    /// <summary>
    /// Looks up knowledge for a specific service by name.
    /// </summary>
    ServiceKnowledgeEntry? FindByName(string serviceName);

    /// <summary>
    /// Looks up knowledge for services on a given port.
    /// </summary>
    IReadOnlyList<ServiceKnowledgeEntry> FindByPort(int port);

    /// <summary>
    /// Gets entries filtered by category.
    /// </summary>
    IReadOnlyList<ServiceKnowledgeEntry> GetByCategory(Enums.ServiceCategory category);

    /// <summary>
    /// Adds a custom entry to the knowledge base.
    /// </summary>
    void AddEntry(ServiceKnowledgeEntry entry);

    /// <summary>
    /// Removes a custom entry from the knowledge base.
    /// </summary>
    bool RemoveEntry(string serviceName);

    /// <summary>
    /// Reloads entries from a JSON configuration file.
    /// </summary>
    Task LoadFromConfigurationAsync(string filePath, CancellationToken ct);
}
