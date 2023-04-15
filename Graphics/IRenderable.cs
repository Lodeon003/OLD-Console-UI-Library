using System.Drawing;

namespace Lodeon.Terminal;

public interface IRenderable
{
    public static byte ColorSimilarityThreshold { get; set; } = 0;
    ReadOnlySpan<Pixel> GetGraphics();
    Rectangle GetScreenArea();
}


