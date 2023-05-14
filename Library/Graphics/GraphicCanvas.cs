using System.Drawing;

namespace Lodeon.Terminal.Graphics;

public class GraphicCanvas
{
    private GraphicBuffer _buffer;
    
    public GraphicCanvas(GraphicBuffer buffer)
    {
        _buffer = buffer;
    }

    public ReadonlyGraphicBuffer AsReadonly()
        => new ReadonlyGraphicBuffer(_buffer);

    public void Fill(Color color)
        => _buffer.Fill(new Pixel().WithBackground(color));

    public void DrawRectangle(Rectangle area, Color background)
        => DrawRectangle(area, new Pixel().WithBackground(background));

    public void DrawRectangle(Rectangle area, Pixel pixel)
    {
        for(int y = area.Top; y < area.Bottom; y++)
            for(int x = area.Left; x < area.Right; x++)
                _buffer.Write(x, y, pixel);
    }

    public void DrawEmptyRectangle(Rectangle area, Color background)
        => DrawEmptyRectangle(area, new Pixel().WithBackground(background));

    public void DrawEmptyRectangle(Rectangle area, Pixel pixel)
    {
        for (int y = area.Top; y < area.Bottom; y++)
        {
            for (int x = area.Left; x < area.Right; x++)
            {
                if ((y != area.Top && y != area.Bottom - 1) && (x != area.Left && x != area.Right - 1))
                    continue;

                _buffer.Write(x, y, pixel);
            }
        }
    }
}