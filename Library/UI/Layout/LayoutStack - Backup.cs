using Lodeon.Terminal.UI.Units;
using System.Buffers;
using System.Runtime.InteropServices;

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

    public void Calculate(Span<PixelLayout> layouts, Span<PixelPoint> positions)
    {
        _currentX = _position.X;
        _currentY = _position.Y;

        int totalSizeX = 0, totalSizeY = 0;

        for (int i = 0; i < layouts.Length; i++)
        {
            totalSizeX += layouts[i].Size.X + layouts[i].Margin.Size.X;
            totalSizeY += layouts[i].Size.Y + layouts[i].Margin.Size.Y;

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

        for(int i = 0; i < positions.Length; i++)
        {
            int sizeX = _orientation == Orientation.Horizontal ? totalSizeX : positions[i].X;
            int sizeY = _orientation == Orientation.Vertical ? totalSizeY : positions[i].Y;

            int offset = 0;
            switch (_horizontal)
            {
                case HorizontalAlign.Left:
                    break;

                case HorizontalAlign.Center:
                    offset = (_size.X - sizeX) / 2;
                    positions[i] = new(positions[i].X + offset, positions[i].Y);
                    break;

                case HorizontalAlign.Right:
                    offset = _size.X - sizeX;
                    positions[i] = new(positions[i].X + offset, positions[i].Y);
                    break;
            }

            switch (_vertical)
            {
                case VerticalAlign.Top:
                    break;

                case VerticalAlign.Center:
                    offset = (_size.Y - sizeY) / 2;
                    positions[i] = new(positions[i].X, positions[i].Y + offset);
                    break;

                case VerticalAlign.Bottom:
                    offset = _size.Y - sizeY;
                    positions[i] = new(positions[i].X, positions[i].Y + offset);
                    break;
            }
        }
    }
    private int UpdateX(PixelPoint size, Pixel4 margin)
    {
        int result = 0;

        // If left or center (center is treated as left but with added offset)
        if (_horizontal != HorizontalAlign.Right)
        {
            _currentX += margin.Left;

            if (_horizontal == HorizontalAlign.Center && _orientation == Orientation.Vertical)
                _currentX += (_size.X / 2 - size.X / 2);

            result = _currentX;

            if (_horizontal == HorizontalAlign.Center && _orientation == Orientation.Vertical)
                _currentX += (_size.X / 2 - size.X / 2);

            if (_orientation == Orientation.Horizontal)
            {
                _currentX += size.X;
                _currentX += margin.Right;
            }
            else
            {
                _currentX -= margin.Left;
            }
            
            return result;
        }
        

        // If RIGHT
        _currentX -= margin.Right;
        _currentX -= size.X;
        result = _currentX;

        if (_orientation == Orientation.Horizontal)
        {
            _currentX -= margin.Left;
        }
        else
        {
            _currentX += margin.Right;
            _currentX += size.X;
        }

        return result;
    }
    private int UpdateY(PixelPoint size, Pixel4 margin)
    {
        int result = 0;
        if (_vertical != VerticalAlign.Bottom)
        {
            _currentY += margin.Top;

            if (_vertical == VerticalAlign.Center)
                _currentY += (_size.Y / 2 - size.Y / 2);

            result = _currentY;

            if (_vertical == VerticalAlign.Center)
                _currentY -= (_size.Y / 2 - size.Y / 2);

            if (_orientation == Orientation.Vertical)
            {
                _currentY += size.Y;
                _currentY += margin.Bottom;
            }
            else
            {
                _currentY -= margin.Top;
            }
            
            return result;
        }
       
        // IF BOTTOM
        _currentY -= margin.Bottom;
        _currentY -= size.Y;
        result = _currentY;

        if (_orientation == Orientation.Vertical)
        {
            _currentY -= margin.Top;
        }
        else
        {
            _currentY += size.Y;
            _currentY += margin.Bottom;
        }

        return result;
    }
}