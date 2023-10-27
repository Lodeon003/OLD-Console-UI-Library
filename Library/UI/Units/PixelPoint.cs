using System.Diagnostics.CodeAnalysis;

namespace Lodeon.Terminal.UI.Units;

/// <summary>
/// Represents an integer point on a 2D surface<br/>(Is implicitly converted from and to a <see cref="System.Drawing.Point"/>)
/// </summary>
public readonly struct PixelPoint
{
    public readonly int X;
    public readonly int Y;

    public static PixelPoint One => new PixelPoint(1, 1);

    public static PixelPoint Zero => new PixelPoint(0, 0);

    public static PixelPoint Max => new PixelPoint(int.MaxValue, int.MaxValue);

    public PixelPoint(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static bool operator ==(PixelPoint a, PixelPoint b)
        => a.X == b.X && a.Y == b.Y;

    public static bool operator !=(PixelPoint a, PixelPoint b)
        => a.X != b.X && a.Y != b.Y;

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        PixelPoint? other = obj as PixelPoint?;

        if (other is not PixelPoint point)
            return false;

        return point.X == this.X && point.Y == this.Y;
    }

    public static PixelPoint FromPoint(Point point, PixelPoint scale)
        => FromPoint(point, scale.X, scale.Y);

    public static PixelPoint FromPoint(Point point, int scaleX, int scaleY)
        => FromPoint(point.X, point.Y, scaleX, scaleY);

    public static PixelPoint FromPoint(Unit x, Unit y, int scaleX, int scaleY)
        => new PixelPoint(PixelPoint.FromUnit(x, scaleX), PixelPoint.FromUnit(y, scaleY));

    public static PixelPoint Clamp(PixelPoint point, PixelPoint min, PixelPoint max)
        => new PixelPoint(Math.Clamp(point.X, min.X, max.X), Math.Clamp(point.Y, min.Y, max.Y));

    public static int FromUnit(Unit unit, int scale)
        => unit.Kind == UnitKind.Pixel ? unit.Value : unit.Value * 100 / scale;

    public static PixelPoint operator +(PixelPoint a, PixelPoint b)
        => new PixelPoint(a.X + b.X, a.Y + b.Y);

    public static implicit operator PixelPoint(System.Drawing.Point point)
        => new PixelPoint(point.X, point.Y);

    public static implicit operator System.Drawing.Point(PixelPoint point)
        => new System.Drawing.Point(point.X, point.Y);

    public override string ToString()
    {
        return $"({X};{Y})";
    }
}
