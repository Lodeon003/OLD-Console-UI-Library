using Lodeon.Terminal.UI.Units;
using System.Diagnostics;
using Lodeon.Terminal;
namespace Lodeon.Terminal.UI.Layout;

public ref struct LayoutStackBackup
{
    public enum HorizontalAlign
    {
        Left,
        Center,
        Right,
    }

    public enum VerticalAlign
    {
        Top,
        Center,
        Bottom,
    }

    public enum Orientation
    {
        Horizontal,
        Vertical
    }


    private PixelPoint _size;
    private PixelPoint _position;
    private int _currentX = 0;
    private int _currentY = 0;
    private HorizontalAlign _horizontal;
    private VerticalAlign _vertical;
    private Orientation _orientation;

    public PixelPoint Position => _position;
    public PixelPoint Size => _size;

    public LayoutStackBackup(PixelPoint position, PixelPoint size, HorizontalAlign horizontalAlign, VerticalAlign verticalAlign, Orientation orientation)
    {
        _position = position;
        _size = size;
        _orientation = orientation;

        _vertical = verticalAlign;
        _horizontal = horizontalAlign;
    }

    public void SetPosition(int x, int y)
    {
        _position = new PixelPoint(x, y);
    }

    public void SetSize(int x, int y)
    {
        _size = new PixelPoint(x, y);
    }

    public void Calculate(Span<PixelLayout> layouts, Span<PixelPoint> positions)
    {
        _currentX = _position.X;
        _currentY = _position.Y;

        int childrenSizeX = 0, childrenSizeY = 0;

        for (int i = 0; i < layouts.Length; i++)
        {
            childrenSizeX += layouts[i].Size.X + layouts[i].Margin.Size.X;
            childrenSizeY += layouts[i].Size.Y + layouts[i].Margin.Size.Y;

            int x = _currentX + layouts[i].Margin.Left;
            int y = _currentY + layouts[i].Margin.Top;

            positions[i] = new PixelPoint(x, y);

            if(_orientation == Orientation.Horizontal)
            {
                _currentX += layouts[i].Size.X + layouts[i].Margin.Right;
                continue;
            }

            _currentY += layouts[i].Size.Y + layouts[i].Margin.Bottom;
        }

        //childrenSizeX = Math.Clamp(childrenSizeX, 0, this._size.X);
        //childrenSizeY = Math.Clamp(childrenSizeY, 0, this._size.Y);

        for (int i = 0; i < positions.Length; i++)
        {
            int offsetX = 0, offsetY = 0;
            PixelPoint wholeSize = layouts[i].Size + layouts[i].Margin.Size;

            if (_orientation == Orientation.Horizontal)
            {
                if(_horizontal == HorizontalAlign.Center)
                    offsetX = (this._size.X - childrenSizeX);
                
                else if (_horizontal == HorizontalAlign.Right)
                    offsetX = (this._size.X - childrenSizeX) * 2;

                if (_vertical == VerticalAlign.Center)
                    offsetY = (this._size.Y - wholeSize.Y) / 2;

                else if (_vertical == VerticalAlign.Bottom)
                    offsetY = (this._size.Y - wholeSize.Y);
            }
            else
            {
                if (_horizontal == HorizontalAlign.Center)
                    offsetX = (this._size.X - wholeSize.X) / 2;

                else if (_horizontal == HorizontalAlign.Right)
                    offsetX = (this._size.X - wholeSize.X);

                if (_vertical == VerticalAlign.Center)
                    offsetY = (this._size.Y - childrenSizeY);

                else if (_vertical == VerticalAlign.Bottom)
                    offsetY = (this._size.Y - childrenSizeY) * 2;
            }

            Debugger.Log($"Whole Position: {this._position}\nWhole size: {this._size}\nChildren size: {childrenSizeX};{childrenSizeY}\nThat position: {positions[i]}\nThat size: {wholeSize}\nOffset: {offsetX},{offsetY}");
            positions[i] = new(positions[i].X + offsetX, positions[i].Y + offsetY);
        }
    }
}