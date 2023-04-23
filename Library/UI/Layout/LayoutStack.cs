using Lodeon.Terminal.UI.Units;

namespace Lodeon.Terminal.UI.Layout;

internal ref struct LayoutStack
{
    private PixelPoint _size;
    private PixelPoint _position;
    private int _currentX = 0;
    private int _currentY = 0;

    public LayoutStack(PixelPoint position, PixelPoint size)
    {
        _position = position;
        _size = size;
    }

    internal PixelPoint Add(PixelPoint size)
    {
        return _position + new PixelPoint(_currentX, _currentY);
    }
}
