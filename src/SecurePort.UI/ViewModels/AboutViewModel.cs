using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SecurePort.UI.ViewModels;

public partial class AboutViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _appName = "SecurePort";

    [ObservableProperty]
    private string _version = string.Empty;

    [ObservableProperty]
    private string _description = "SecurePort is a modern network port scanning tool built with Avalonia UI for cross-platform desktop environments.";

    [ObservableProperty]
    private string _license = "MIT License";

    [ObservableProperty]
    private string _privacyNotice = "SecurePort scans only the targets you explicitly specify. No data is transmitted externally. All scan history is stored locally on your device.";

    public IReadOnlyList<CreditInfo> Credits { get; } = new List<CreditInfo>
    {
        new("Avalonia UI", "Cross-platform .NET UI framework"),
        new("CommunityToolkit.Mvvm", "MVVM helper library for .NET"),
        new("Fluent Theme", "Modern design system"),
    };

    public IReadOnlyList<string> OpenSourceAttributions { get; } = new List<string>
    {
        "Avalonia UI - MIT License",
        "CommunityToolkit.Mvvm - MIT License",
        "Avalonia.Themes.Fluent - MIT License",
        "Avalonia.Fonts.Inter - SIL Open Font License",
    };

    public AboutViewModel()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        _version = version?.ToString(3) ?? "1.0.0";
    }
}

public sealed record CreditInfo(string Name, string Description);
