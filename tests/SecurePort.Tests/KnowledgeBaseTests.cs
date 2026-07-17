using SecurePort.Core.Enums;
using SecurePort.Core.Models;
using SecurePort.Results.KnowledgeBase;

namespace SecurePort.Tests;

public class ServiceKnowledgeBaseTests
{
    private readonly ServiceKnowledgeBase _kb = new();

    [Fact]
    public void Constructor_PopulatesDefaultEntries()
    {
        var entries = _kb.GetAllEntries();
        Assert.True(entries.Count >= 15);
    }

    [Fact]
    public void FindByName_CaseInsensitive()
    {
        var entry = _kb.FindByName("ssh");
        Assert.NotNull(entry);
        Assert.Equal("SSH", entry.ServiceName);
    }

    [Fact]
    public void FindByName_NullWhitespace_ReturnsNull()
    {
        Assert.Null(_kb.FindByName(null!));
        Assert.Null(_kb.FindByName("   "));
    }

    [Fact]
    public void FindByName_NotFound_ReturnsNull()
    {
        Assert.Null(_kb.FindByName("nonexistent"));
    }

    [Fact]
    public void FindByPort_ReturnsMatchingEntries()
    {
        var entries = _kb.FindByPort(80);
        Assert.NotEmpty(entries);
        Assert.All(entries, e => Assert.Contains(80, e.DefaultPorts));
    }

    [Fact]
    public void FindByPort_NoMatch_ReturnsEmpty()
    {
        var entries = _kb.FindByPort(99999);
        Assert.Empty(entries);
    }

    [Fact]
    public void GetByCategory_ReturnsMatchingEntries()
    {
        var entries = _kb.GetByCategory(ServiceCategory.Remote);
        Assert.NotEmpty(entries);
        Assert.All(entries, e => Assert.Equal(ServiceCategory.Remote, e.Category));
    }

    [Fact]
    public void AddEntry_NewEntry_AddsSuccessfully()
    {
        var entry = new ServiceKnowledgeEntry
        {
            ServiceName = "CustomService",
            DefaultPorts = new[] { 12345 },
            Protocol = ProtocolType.TCP,
            Description = "A custom service",
            TypicalUsage = "Testing",
            CommonAlternatives = new List<string>(),
            Category = ServiceCategory.Custom
        };
        _kb.AddEntry(entry);
        Assert.NotNull(_kb.FindByName("CustomService"));
    }

    [Fact]
    public void AddEntry_ExistingName_ReplacesEntry()
    {
        var original = _kb.FindByName("SSH");
        Assert.NotNull(original);

        var replacement = new ServiceKnowledgeEntry
        {
            ServiceName = "SSH",
            DefaultPorts = new[] { 22, 2222 },
            Protocol = ProtocolType.TCP,
            Description = "Updated SSH",
            TypicalUsage = "Remote access",
            CommonAlternatives = new List<string>(),
            Category = ServiceCategory.Remote
        };
        _kb.AddEntry(replacement);
        var updated = _kb.FindByName("SSH");
        Assert.Equal("Updated SSH", updated!.Description);
        Assert.Contains(2222, updated.DefaultPorts);
    }

    [Fact]
    public void RemoveEntry_ExistingName_ReturnsTrue()
    {
        Assert.True(_kb.RemoveEntry("SSH"));
        Assert.Null(_kb.FindByName("SSH"));
    }

    [Fact]
    public void RemoveEntry_NotFound_ReturnsFalse()
    {
        Assert.False(_kb.RemoveEntry("nonexistent"));
    }

    [Fact]
    public void RemoveEntry_NullWhitespace_ReturnsFalse()
    {
        Assert.False(_kb.RemoveEntry(null!));
        Assert.False(_kb.RemoveEntry("   "));
    }

    [Fact]
    public async Task LoadFromConfigurationAsync_NullPath_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _kb.LoadFromConfigurationAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task LoadFromConfigurationAsync_WhitespacePath_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _kb.LoadFromConfigurationAsync("   ", CancellationToken.None));
    }

    [Fact]
    public async Task LoadFromConfigurationAsync_NonExistentFile_ReturnsSilently()
    {
        await _kb.LoadFromConfigurationAsync("/tmp/nonexistent_file_12345.json", CancellationToken.None);
        var count = _kb.GetAllEntries().Count;
        Assert.True(count >= 15);
    }
}
