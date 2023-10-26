using Lodeon.Terminal.UI.Units;
using System.Drawing;

namespace Lodeon.Terminal;

public interface IRenderable
{
    public static byte ColorSimilarityThreshold { get; set; } = 0;
    ReadOnlySpan<Pixel> GetGraphics();
    Pixel4 GetArea();
}


