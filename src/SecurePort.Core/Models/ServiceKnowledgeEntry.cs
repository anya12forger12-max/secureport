using SecurePort.Core.Enums;

namespace SecurePort.Core.Models;

/// <summary>
/// A knowledge base entry describing a well-known network service.
/// </summary>
public sealed record ServiceKnowledgeEntry
{
    /// <summary>The common service name.</summary>
    public required string ServiceName { get; init; }

    /// <summary>Default port numbers associated with this service.</summary>
    public required IReadOnlyList<int> DefaultPorts { get; init; }

    /// <summary>The primary protocol used.</summary>
    public required ProtocolType Protocol { get; init; }

    /// <summary>A brief description of the service.</summary>
    public required string Description { get; init; }

    /// <summary>Typical usage scenarios.</summary>
    public required string TypicalUsage { get; init; }

    /// <summary>Common alternative services or protocols.</summary>
    public required IReadOnlyList<string> CommonAlternatives { get; init; }

    /// <summary>The functional category this service belongs to.</summary>
    public required ServiceCategory Category { get; init; }
}
