namespace SecurePort.Themes.Engines;

/// <summary>
/// Represents an ARGB color within a theme palette.
/// </summary>
/// <param name="A">Alpha channel (0-255).</param>
/// <param name="R">Red channel (0-255).</param>
/// <param name="G">Green channel (0-255).</param>
/// <param name="B">Blue channel (0-255).</param>
public sealed record ThemeColor(byte A, byte R, byte G, byte B)
{
    /// <summary>
    /// Gets the color as a hex string in the format "#RRGGBB".
    /// </summary>
    public string ToHex() => $"#{R:X2}{G:X2}{B:X2}";

    /// <summary>
    /// Gets the color as a hex string in the format "#AARRGGBB".
    /// </summary>
    public string ToArgbHex() => $"#{A:X2}{R:X2}{G:X2}{B:X2}";

    /// <summary>
    /// Creates a <see cref="ThemeColor"/> from a hex string (e.g. "#FF5722" or "#FF00FF00").
    /// </summary>
    /// <param name="hex">The hex color string.</param>
    /// <returns>A new <see cref="ThemeColor"/> instance.</returns>
    /// <exception cref="FormatException">Thrown when the hex string is not in a valid format.</exception>
    public static ThemeColor FromHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            throw new ArgumentException("Hex color string must not be null or empty.", nameof(hex));

        hex = hex.TrimStart('#');

        if (hex.Length == 6)
        {
            var r = Convert.ToByte(hex[..2], 16);
            var g = Convert.ToByte(hex[2..4], 16);
            var b = Convert.ToByte(hex[4..6], 16);
            return new ThemeColor(0xFF, r, g, b);
        }

        if (hex.Length == 8)
        {
            var a = Convert.ToByte(hex[..2], 16);
            var r = Convert.ToByte(hex[2..4], 16);
            var g = Convert.ToByte(hex[4..6], 16);
            var b = Convert.ToByte(hex[6..8], 16);
            return new ThemeColor(a, r, g, b);
        }

        throw new FormatException($"Invalid hex color format: '{hex}'. Expected 6 or 8 hex digits.");
    }
}
