namespace Lodeon.Terminal;

public struct Pixel
{
    public static char EmptyCharacter { get; } = ' ';
    public static Pixel White { get; } = new Pixel();
    public static Pixel Invisible { get; } = new Pixel(' ', Color.Invisible, Color.Invisible);
    //public static Rectangle Size { get; } = new Rectangle(0, 0, 1, 1);

    public static Color DefaultForeground { get; set; } = new Color(255, 255, 255);
    public static Color DefaultBackground { get; set; } = new Color(0, 0, 0);

    public char Char { get; private set; } = EmptyCharacter;
    public Color Foreground { get; private set; } = DefaultForeground;
    public Color Background { get; private set; } = DefaultBackground;

    /// <summary>
    /// Returns wether this pixel displays a foreground character and foreground is visible (not fully transparent)
    /// </summary>
    public bool HasForeground => Char != EmptyCharacter && Foreground.IsVisible;
    /// <summary>
    /// Returns wether background is visible (not fully transparent)
    /// </summary>
    public bool HasBackground => Background.IsVisible;

    /// <summary>
    /// This constructor returns a pixel with a character (displayed using foreground color) on a background color
    /// </summary>
    /// <param name="c"></param>
    /// <param name="fore"></param>
    /// <param name="back"></param>
    public Pixel(char c, Color fore, Color back)
    {
        Char = c;
        Foreground = fore;
        Background = back;
    }

    /// <summary>
    /// This constructor returns a 'color' pixel wich will display a color.<br/>This pixel will not display a character and foreground color will be ignored
    /// </summary>
    ///<param name="color">The background color of the pixel</param>
    public Pixel(Color color)
    {
        Background = color;
        Foreground = Color.Invisible;

        Char = EmptyCharacter;
    }

    /// <summary>
    /// This constructor returns a fully transparent (and invisible) pixel
    /// </summary>
    public Pixel()
    {
        Foreground = Color.Invisible;
        Background = Color.Invisible;

        Char = EmptyCharacter;
    }

    public Pixel WithChar(char character)
    => new Pixel(character, Foreground, Background);

    public Pixel WithForeground(Color foreground)
        => new Pixel(this.Char, foreground, this.Background);

    public Pixel WithForeground(byte red, byte green, byte blue, byte alpha)
        => new Pixel(this.Char, new Color(red, green, blue, alpha), this.Background);

    public Pixel WithForeground(byte red, byte green, byte blue)
        => new Pixel(this.Char, new Color(red, green, blue), this.Background);

    public Pixel WithBackground(Color background)
        => new Pixel(this.Char, this.Foreground, background);

    public Pixel WithBackground(byte red, byte green, byte blue, byte alpha)
    => new Pixel(this.Char, this.Foreground, new Color(red, green, blue, alpha));

    public Pixel WithBackground(byte red, byte green, byte blue)
        => new Pixel(this.Char, this.Foreground, new Color(red, green, blue));

    public static Pixel Overlay(Pixel @base, Pixel overlay)
        => @base.Overlay(overlay);

    /// <summary>
    /// Overlays one pixel to this instance. The overlaying follows these rules:
    /// <list type="bullet">
    /// <item><see cref="Background"/> -> The <paramref name="other"/>'s background is layed on top of this background</item>
    /// <item><see cref="Foreground"/> -> The <paramref name="other"/>'s foreground is layed on top of the resulting background</item>
    /// <item><see cref="Char"/> -> The character is <paramref name="other"/>'s character</item>
    /// </list>
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public Pixel Overlay(Pixel other)
    {
        Color background = Color.Overlay(Background, other.Background);
        Color foreground = Color.Overlay(background, other.Foreground);

        return new Pixel(other.Char, foreground, background);
    }

    public override string ToString()
    {
        return $"'{Char}' - {Foreground} - {Background}";
    }
}


