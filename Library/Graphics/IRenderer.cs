namespace Lodeon.Terminal;

public interface IRenderer
{
    void Display(IRenderable graphic)
        => Display(graphic, 1);

    void Display(IRenderable graphic, byte ColorSimilarityThreshold);
}


