using Lodeon.Terminal.UI.Units;
using System.Buffers;
using System.Runtime.InteropServices;

namespace Lodeon.Terminal.UI.Layout;

public ref struct LayoutStack
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

    public LayoutStack(PixelPoint position, PixelPoint size, HorizontalAlign horizontalAlign, VerticalAlign verticalAlign, Orientation orientation)
    {
        _position = position;
        _size = size;
        _orientation = orientation;

        _vertical = verticalAlign;
        _horizontal = horizontalAlign;
    }

    public void Calculate(Span<PixelLayout> layouts, Span<PixelPoint> positions)
    {
        _currentX = _horizontal switch
        {
            HorizontalAlign.Left => _position.X,
            HorizontalAlign.Center => _position.X,   // Centering will be handled later
            HorizontalAlign.Right => _position.X + _size.X,
            _ => throw new NotImplementedException(),
        };

        _currentY = _vertical switch
        {
            VerticalAlign.Top => _position.Y,
            VerticalAlign.Center => _position.Y,   // Centering will be handled later
            VerticalAlign.Bottom => _position.Y + _size.Y,
            _ => throw new NotImplementedException(),
        };

        int totalSizeX = 0, totalSizeY = 0;
        for (int i = 0; i < layouts.Length; i++)
        {
            totalSizeX += layouts[i].Size.X + layouts[i].Margin.Size.X;
            totalSizeY += layouts[i].Size.Y + layouts[i].Margin.Size.Y;
        }

        if (_horizontal == HorizontalAlign.Center && _orientation == Orientation.Horizontal)
            _currentX += (_size.X - totalSizeX) / 2;

        if (_vertical == VerticalAlign.Center && _orientation == Orientation.Vertical)
            _currentY += (_size.Y - totalSizeY) / 2;

        // Invert loop order if starting from right or bottom
        if (_orientation == Orientation.Horizontal && _horizontal == HorizontalAlign.Right ||
        _orientation == Orientation.Vertical && _vertical == VerticalAlign.Bottom)
        {
            for (int i = layouts.Length-1; i >= 0; i--)
            {
                int x = UpdateX(layouts[i].Size, layouts[i].Margin);
                int y = UpdateY(layouts[i].Size, layouts[i].Margin);
                positions[i] = new PixelPoint(x, y);
            }
        }
        else
        {
            for (int i = 0; i < layouts.Length; i++)
            {
                int x = UpdateX(layouts[i].Size, layouts[i].Margin);
                int y = UpdateY(layouts[i].Size, layouts[i].Margin);
                positions[i] = new PixelPoint(x, y);
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
            
            result = _currentX;
            
            if(_orientation == Orientation.Horizontal)
            {
                _currentX += size.X;
                _currentX += margin.Right;
            }
            else
                _currentX -= margin.Left;
            
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
            result = _currentY;

            if (_orientation == Orientation.Vertical)
            {
                _currentY += size.Y;
                _currentY += margin.Bottom;
            }
            else
                _currentY -= margin.Top;
            
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

public struct PixelLayout
{
    public PixelPoint Size { get; set; }
    public Pixel4 Margin { get; set; }

    public PixelLayout(PixelPoint size, Pixel4 margin)
    {
        Size = size;
        Margin = margin;
    }
}