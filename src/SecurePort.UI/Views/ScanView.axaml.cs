using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SecurePort.UI.Views;

public partial class ScanView : UserControl
{
    public ScanView()
    {
        InitializeComponent();
    }

    private void OnEmptyStateChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is Border border)
        {
            border.IsVisible = e.NewValue is true;
        }
    }
}
