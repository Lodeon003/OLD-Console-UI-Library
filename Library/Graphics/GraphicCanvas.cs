using Microsoft.VisualBasic;
using System.Drawing;

namespace Lodeon.Terminal.Graphics;

public class GraphicCanvas
{
    private GraphicBuffer _buffer;
    
    public GraphicCanvas(GraphicBuffer buffer)
        => _buffer = buffer;

    public ReadonlyGraphicBuffer AsReadonly()
        => new ReadonlyGraphicBuffer(_buffer);


    public void Fill(Color color)
        => _buffer.Fill(new Pixel().WithBackground(color));

    public void DrawText(Span<char> text, int x, int y, Color foreground)
        => DrawText(text, x, y, foreground, Color.Invisible);

    public void DrawText(Span<char> text, int x, int y, Color foreground, Color background)
    {
        int buffPosition = x * y;
        int buffLength = _buffer.Length;
        int buffCyles = buffLength - buffPosition;

        if (buffCyles < 0)
            return;

        int cycles = Math.Min(buffCyles, text.Length);

        for (int i = 0; i < cycles; i++)
        {
            _buffer.Overlay(new Pixel(text[i], foreground, background));
            buffPosition++;
        }
    }

    public void DrawRectangle(Rectangle area, Color background)
        => DrawRectangle(area, new Pixel().WithBackground(background));


    public void DrawRectangle(Rectangle area, Pixel pixel)
    {
        for(int y = area.Top; y < area.Bottom; y++)
            for(int x = area.Left; x < area.Right; x++)
                _buffer.Write(x, y, pixel);
    }

    public void DrawColumn(int column, Color background)
        => DrawColumn(column, new Pixel().WithBackground(background));

    public void DrawColumn(int column, Pixel pixel)
    {
        for (int y = 0; y < _buffer.Height; y++)
            _buffer.Write(column, y, pixel);
    }

    public void DrawRow(int row, Color background)
        => DrawRow(row, new Pixel().WithBackground(background));

    public void DrawRow(int row, Pixel pixel)
    {
        for (int x = 0; x < _buffer.Height; x++)
            _buffer.Write(x, row, pixel);
    }

    public void DrawBorder(Rectangle area, Color background)
        => DrawBorder(area, new Pixel().WithBackground(background));

    public void DrawBorder(Rectangle area, Pixel pixel)
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