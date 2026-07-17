using System.Text.Json;
using SecurePort.Core.Enums;
using SecurePort.Core.Interfaces;
using SecurePort.Core.Models;

namespace SecurePort.Results.KnowledgeBase;

/// <summary>
/// Maintains a local knowledge base of well-known network services.
/// Supports loading from JSON configuration files for easy updates.
/// </summary>
public sealed class ServiceKnowledgeBase : IServiceKnowledgeBase
{
    private readonly List<ServiceKnowledgeEntry> _entries = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public ServiceKnowledgeBase()
    {
        SeedDefaultEntries();
    }

    public IReadOnlyList<ServiceKnowledgeEntry> GetAllEntries()
    {
        lock (_entries)
        {
            return _entries.ToList();
        }
    }

    public ServiceKnowledgeEntry? FindByName(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            return null;

        lock (_entries)
        {
            return _entries.FirstOrDefault(e =>
                e.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));
        }
    }

    public IReadOnlyList<ServiceKnowledgeEntry> FindByPort(int port)
    {
        lock (_entries)
        {
            return _entries.Where(e => e.DefaultPorts.Contains(port)).ToList();
        }
    }

    public IReadOnlyList<ServiceKnowledgeEntry> GetByCategory(ServiceCategory category)
    {
        lock (_entries)
        {
            return _entries.Where(e => e.Category == category).ToList();
        }
    }

    public void AddEntry(ServiceKnowledgeEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        lock (_entries)
        {
            var existing = _entries.FirstOrDefault(e =>
                e.ServiceName.Equals(entry.ServiceName, StringComparison.OrdinalIgnoreCase));

            if (existing is not null)
            {
                _entries.Remove(existing);
            }

            _entries.Add(entry);
        }
    }

    public bool RemoveEntry(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
            return false;

        lock (_entries)
        {
            var entry = _entries.FirstOrDefault(e =>
                e.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));

            if (entry is null)
                return false;

            return _entries.Remove(entry);
        }
    }

    public async Task LoadFromConfigurationAsync(string filePath, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            return;

        var json = await File.ReadAllTextAsync(filePath, ct);
        if (string.IsNullOrWhiteSpace(json))
            return;

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        var entries = JsonSerializer.Deserialize<List<ServiceKnowledgeEntry>>(json, options);
        if (entries is null)
            return;

        await _lock.WaitAsync(ct);
        try
        {
            foreach (var entry in entries)
            {
                var existing = _entries.FirstOrDefault(e =>
                    e.ServiceName.Equals(entry.ServiceName, StringComparison.OrdinalIgnoreCase));

                if (existing is not null)
                    _entries.Remove(existing);

                _entries.Add(entry);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    private void SeedDefaultEntries()
    {
        _entries.Add(new ServiceKnowledgeEntry
        {
            ServiceName = "SSH",
            DefaultPorts = new[] { 22 },
            Protocol = ProtocolType.TCP,
            Description = "Secure Shell remote access protocol",
            TypicalUsage = "Remote server administration, secure file transfer, tunneling",
            CommonAlternatives = new[] { "Telnet", "RDP", "VNC" },
            Category = ServiceCategory.Remote
        });

        _entries.Add(new ServiceKnowledgeEntry
        {
            ServiceName = "HTTP",
            DefaultPorts = new[] { 80, 8080, 8000 },
            Protocol = ProtocolType.TCP,
            Description = "Hypertext Transfer Protocol",
            TypicalUsage = "Web browsing, REST APIs, web applications",
            CommonAlternatives = new[] { "HTTPS" },
            Category = ServiceCategory.Web
        });

        _entries.Add(new ServiceKnowledgeEntry
        {
            ServiceName = "HTTPS",
            DefaultPorts = new[] { 443, 8443 },
            Protocol = ProtocolType.TCP,
            Description = "Hypertext Transfer Protocol Secure",
            TypicalUsage = "Secure web browsing, encrypted APIs, secure web applications",
            CommonAlternatives = new[] { "HTTP" },
            Category = ServiceCategory.Web
        });

        _entries.Add(new ServiceKnowledgeEntry
        {
            ServiceName = "FTP",
            DefaultPorts = new[] { 20, 21 },
            Protocol = ProtocolType.TCP,
            Description = "File Transfer Protocol",
            TypicalUsage = "File transfers, website deployment",
            CommonAlternatives = new[] { "SFTP", "SCP", "TFTP" },
            Category = ServiceCategory.FileTransfer
        });

        _entries.Add(new ServiceKnowledgeEntry
        {
            ServiceName = "MySQL",
            DefaultPorts = new[] { 3306 },
            Protocol = ProtocolType.TCP,
            Description = "MySQL Database Server",
            TypicalUsage = "Relational database management for web applications",
            CommonAlternatives = new[] { "PostgreSQL", "MariaDB" },
            Category = ServiceCategory.Database
        });

        _entries.Add(new ServiceKnowledgeEntry
        {
            ServiceName = "PostgreSQL",
            DefaultPorts = new[] { 5432 },
            Protocol = ProtocolType.TCP,
            Description = "PostgreSQL Database Server",
            TypicalUsage = "Advanced relational database management",
            CommonAlternatives = new[] { "MySQL", "MariaDB" },
            Category = ServiceCategory.Database
        });

        _entries.Add(new ServiceKnowledgeEntry
        {
            ServiceName = "RDP",
            DefaultPorts = new[] { 3389 },
            Protocol = ProtocolType.TCP,
            Description = "Remote Desktop Protocol",
            TypicalUsage = "Windows remote desktop access",
            CommonAlternatives = new[] { "VNC", "SSH", "TeamViewer" },
            Category = ServiceCategory.Remote
        });

        _entries.Add(new ServiceKnowledgeEntry
        {
            ServiceName = "DNS",
            DefaultPorts = new[] { 53 },
            Protocol = ProtocolType.UDP,
            Description = "Domain Name System",
            TypicalUsage = "Domain name resolution",
            CommonAlternatives = new[] { "mDNS" },
            Category = ServiceCategory.System
        });

        _entries.Add(new ServiceKnowledgeEntry
        {
            ServiceName = "Redis",
            DefaultPorts = new[] { 6379 },
            Protocol = ProtocolType.TCP,
            Description = "Redis Key-Value Store",
            TypicalUsage = "Caching, session storage, message brokering",
            CommonAlternatives = new[] { "Memcached", "MongoDB" },
            Category = ServiceCategory.Database
        });

        _entries.Add(new ServiceKnowledgeEntry
        {
            ServiceName = "MongoDB",
            DefaultPorts = new[] { 27017 },
            Protocol = ProtocolType.TCP,
            Description = "MongoDB Document Database",
            TypicalUsage = "NoSQL document storage for modern applications",
            CommonAlternatives = new[] { "CouchDB", "DynamoDB" },
            Category = ServiceCategory.Database
        });

        _entries.Add(new ServiceKnowledgeEntry
        {
            ServiceName = "SMTP",
            DefaultPorts = new[] { 25, 587 },
            Protocol = ProtocolType.TCP,
            Description = "Simple Mail Transfer Protocol",
            TypicalUsage = "Email sending and routing",
            CommonAlternatives = new[] { "ESMTP" },
            Category = ServiceCategory.Mail
        });

        _entries.Add(new ServiceKnowledgeEntry
        {
            ServiceName = "VNC",
            DefaultPorts = new[] { 5900 },
            Protocol = ProtocolType.TCP,
            Description = "Virtual Network Computing",
            TypicalUsage = "Remote desktop access and control",
            CommonAlternatives = new[] { "RDP", "SSH", "TeamViewer" },
            Category = ServiceCategory.Remote
        });

        _entries.Add(new ServiceKnowledgeEntry
        {
            ServiceName = "MSSQL",
            DefaultPorts = new[] { 1433 },
            Protocol = ProtocolType.TCP,
            Description = "Microsoft SQL Server",
            TypicalUsage = "Enterprise relational database management",
            CommonAlternatives = new[] { "MySQL", "PostgreSQL", "Oracle DB" },
            Category = ServiceCategory.Database
        });

        _entries.Add(new ServiceKnowledgeEntry
        {
            ServiceName = "SNMP",
            DefaultPorts = new[] { 161 },
            Protocol = ProtocolType.UDP,
            Description = "Simple Network Management Protocol",
            TypicalUsage = "Network device monitoring and management",
            CommonAlternatives = new[] { "NETCONF", "gNMI" },
            Category = ServiceCategory.System
        });

        _entries.Add(new ServiceKnowledgeEntry
        {
            ServiceName = "Oracle DB",
            DefaultPorts = new[] { 1521 },
            Protocol = ProtocolType.TCP,
            Description = "Oracle Database Listener",
            TypicalUsage = "Enterprise relational database management",
            CommonAlternatives = new[] { "MSSQL", "PostgreSQL" },
            Category = ServiceCategory.Database
        });
    }
}
