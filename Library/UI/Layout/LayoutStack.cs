using Lodeon.Terminal.UI.Units;

namespace Lodeon.Terminal.UI.Layout;

public ref struct LayoutStack
{
    public enum HorizontalAlign
    {
        Left,
        Right,
        Center,
    }

    public enum VerticalAlign
    {
        Top,
        Bottom,
        Center,
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
    private HorizontalAlign _horiziontal;
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
        _horiziontal = horizontalAlign;

        _currentX = horizontalAlign switch
        {
            HorizontalAlign.Left => position.X,
            HorizontalAlign.Right => position.X + size.X,
            _ => throw new NotImplementedException(),
        };

        _currentY = verticalAlign switch
        {
            VerticalAlign.Top => position.Y,
            VerticalAlign.Bottom => position.Y + size.Y,
            _ => throw new NotImplementedException(),
        };
    }

    /// <summary>
    /// Returns the position of an element with a size and margin if it was to be added to the layout.<br/>
    /// Adding items will change the position of next item added
    /// </summary>
    /// <param name="size">The size in pixels of the element to add</param>
    /// <param name="margin">The margin in pixel of the element to add</param>
    /// <returns></returns>
    public PixelPoint Add(PixelPoint size, Pixel4 margin)
    {
        // Add initial margin
        if(_horiziontal == HorizontalAlign.Left)
        {
            _currentX += margin.Left;
        }
        else if (_horiziontal == HorizontalAlign.Right)
        {
            _currentX -= margin.Right;
            _currentX -= margin.Left;
        }

        if (_vertical == VerticalAlign.Top)
        {
            _currentY += margin.Top;
        }
        else if (_vertical == VerticalAlign.Bottom)
        {
            _currentY -= margin.Bottom;
            _currentY -= size.Y;
        }

        PixelPoint result = new PixelPoint(_currentX, _currentY);


        // Add end margin
        if (_horiziontal == HorizontalAlign.Left)
        {
            if(_orientation == Orientation.Horizontal)
            {
                _currentX += margin.Right;
                _currentX += size.X;
            }
            else if (_orientation == Orientation.Vertical)
            {
                _currentX -= margin.Left;
            }
        }
        else if (_horiziontal == HorizontalAlign.Right)
        {
            if (_orientation == Orientation.Horizontal)
            {
                _currentX -= size.X;
            }
            else if (_orientation == Orientation.Vertical)
            {
                _currentX += margin.Right;
            }
        }

        if (_vertical == VerticalAlign.Top)
        {
            if(_orientation == Orientation.Vertical)
            {
                _currentY += size.Y;
                _currentY += margin.Bottom;
            }
            else if (_orientation == Orientation.Horizontal)
            {
                _currentY -= margin.Top;
            }
        }
        else if (_vertical == VerticalAlign.Bottom)
        {
            if (_orientation == Orientation.Vertical)
            {
                _currentY -= margin.Top;
            }
            else
            {
                _currentY += margin.Bottom;
            }
        }

        return result;
    }
}