namespace Lodeon.Terminal;

public struct Color
{
    public static Color Invisible => new Color(0, 0, 0, 0);

    public static Color Black => new Color(0, 0, 0, 255);
    public static Color White => new Color(255, 255, 255, 255);

    public readonly byte Red = 0;
    public readonly byte Green = 0;
    public readonly byte Blue = 0;
    public readonly byte Alpha = 255;

    /// <summary>
    /// Returns wether this color has an <see cref="Alpha"/> of 255 (max value)
    /// </summary>
    public bool IsOpaque => Alpha == 255;
    /// <summary>
    /// Returns whether this color has an <see cref="Alpha"/> value <b>different</b> from <b>zero</b>
    /// </summary>
    public bool IsVisible => Alpha != 0;

    /// <summary>
    /// Returns wether this color is visible and not opaque <code>IsVisible and !IsOpaque</code>
    /// </summary>
    public bool IsTransparent => IsVisible && !IsOpaque;

    /// <summary>
    /// Same as calling <see cref="Color.FromRGBA(byte, byte, byte, byte)"/>
    /// </summary>
    public Color(byte red, byte green, byte blue, byte alpha)
    {
        Red = red;
        Green = green;
        Blue = blue;
        Alpha = alpha;
    }

    /// <summary>
    /// Same as calling <see cref="Color.FromRGB(byte, byte, byte)"/>
    /// </summary>
    public Color(byte red, byte green, byte blue)
    {
        Red = red;
        Green = green;
        Blue = blue;
        Alpha = 255;
    }

    /// <summary>
    /// This constructor returns a fully transparent (and invisible) color
    /// </summary>
    public Color()
    {
        Red = 0;
        Green = 0;
        Blue = 0;
        Alpha = 0;
    }

    public static Color FromHex(string hex)
    {
        byte offset = hex.StartsWith('#') ? (byte)1 : (byte)0;

        if (hex.Length < 6 || hex.Length > 8)
            throw new ArgumentException(nameof(hex));

        byte red = Convert.ToByte(hex.Substring(0 + offset, 2), 16);
        byte green = Convert.ToByte(hex.Substring(2 + offset, 2), 16);
        byte blue = Convert.ToByte(hex.Substring(4 + offset, 2), 16);

        // IF hex supports transparency calculate it, otherwise make color opaque
        byte alpha = hex.Length == 8 ? Convert.ToByte(hex.Substring(6 + offset, 2), 16) : (byte)255;

        return new Color(red, green, blue, alpha);
    }

    public static Color FromRGBA(byte red, byte green, byte blue, byte alpha)
        => new Color(red, green, blue, alpha);

    public static Color FromRGB(byte red, byte green, byte blue)
        => new Color(red, green, blue, 255);


    public bool IsSimilar(Color c, byte threshold)
    {
        return Math.Abs(Red - c.Red) <= threshold &&
            Math.Abs(Green - c.Green) <= threshold &&
            Math.Abs(Blue - c.Blue) <= threshold &&
            Math.Abs(Alpha - c.Alpha) <= threshold;
    }

    public Color Transparent(byte transparency)
        => new Color(Red, Green, Blue, transparency);

    public Color Opaque()
        => new Color(Red, Green, Blue, 255);

    public static Color Overlay(Color baseColor, Color overlayColor)
        => baseColor.Overlay(overlayColor);

    /// <summary>
    /// [!] To Test. Probably overlfows happening
    /// </summary>
    public Color Overlay(Color overlayColor)
    {
        if (!this.IsVisible || overlayColor.IsOpaque)
            return overlayColor;

        checked
        {
            byte resAlpha = (byte)(255 - (255 - Alpha) * (255 - overlayColor.Alpha) / 255);

            byte red = (byte)(overlayColor.Red * overlayColor.Alpha / resAlpha +
                              Red * Alpha * (255 - overlayColor.Alpha) / 255 / resAlpha);

            byte blue = (byte)(overlayColor.Blue * overlayColor.Alpha / resAlpha +
                               Blue * Alpha * (255 - overlayColor.Alpha) / 255 / resAlpha);

            byte green = (byte)(overlayColor.Green * overlayColor.Alpha / resAlpha +
                               Green * Alpha * (255 - overlayColor.Alpha) / 255 / resAlpha);

            return Color.FromRGBA(red, green, blue, resAlpha);
        }
    }

    public override string ToString()
    {
        return $"RGBA: {Red},{Green},{Blue},{Alpha}";
    }
}


