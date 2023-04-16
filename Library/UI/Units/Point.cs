namespace Lodeon.Terminal.UI.Units;

public readonly struct Point
{
    public static Point Empty => new Point(0, UnitKind.Pixel, 0, UnitKind.Pixel);

    public static Point One => new Point(1, UnitKind.Pixel, 1, UnitKind.Pixel);

    public readonly Unit X;
    public readonly Unit Y;

    public Point(int x, UnitKind tX, int y, UnitKind tY)
    {
        X = new Unit(x, tX);
        Y = new Unit(y, tY);
    }

    public static bool operator ==(Point left, Point right)
    {
        return left.X == right.X && left.Y == right.Y;
    }

    public static bool operator !=(Point left, Point right)
    {
        return !(left == right);
    }
}
