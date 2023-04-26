using System.Drawing;

namespace Lodeon.Terminal.UI.Units;

public readonly struct Pixel4
{
    public readonly int Left;
    public readonly int Top;
    public readonly int Right;
    public readonly int Bottom;

    public PixelPoint TopLeft => new PixelPoint(Left, Top);

    /// <summary>
    /// Returns a point with sums of horizontal values as X and sum of vertical values as Y
    /// </summary>
    public PixelPoint Size => new PixelPoint(Right + Left, Bottom + Top);

    /// <summary>
    /// Returns it's size subtracting it's position
    /// </summary>
    public PixelPoint RectSize => new PixelPoint(Right - Left, Bottom - Top);

    public static Pixel4 Zero => new Pixel4(0, 0, 0, 0);

    public static Pixel4 Max => new Pixel4(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);

    public Pixel4(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public Pixel4(PixelPoint topLeft, PixelPoint size)
    {
        Left = topLeft.X;
        Top = topLeft.Y;
        Right = topLeft.X + size.X;
        Bottom = topLeft.Y + size.Y;
    }

    public static Pixel4 FromPoint(Point4 point, Pixel4 scale)
    {
        return new Pixel4(PixelPoint.FromUnit(point.Left, scale.Left),
                            PixelPoint.FromUnit(point.Top, scale.Top),
                            PixelPoint.FromUnit(point.Right, scale.Right),
                            PixelPoint.FromUnit(point.Bottom, scale.Bottom));
    }

    /// <summary>
    /// Scales a point applying <paramref name="scale"/>'s X to <paramref name="point"/>'s horizontal components and <paramref name="scale"/>'s Y to <paramref name="point"/>'s vertical components<br/>
    /// LEft, Right -> X<br/>
    /// Top, Bottom -> Y
    /// </summary>
    /// <param name="point"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    public static Pixel4 FromPoint(Point4 point, PixelPoint scale)
    {
        return new Pixel4(PixelPoint.FromUnit(point.Left, scale.X),
                            PixelPoint.FromUnit(point.Top, scale.Y),
                            PixelPoint.FromUnit(point.Right, scale.X),
                            PixelPoint.FromUnit(point.Bottom, scale.Y));
    }

    public static Pixel4 Clamp(Pixel4 value, Pixel4 min, Pixel4 max)
    {
        return new Pixel4(Math.Clamp(value.Left, min.Left, max.Left),
                          Math.Clamp(value.Top, min.Top, max.Top),
                          Math.Clamp(value.Right, min.Right, max.Right),
                          Math.Clamp(value.Bottom, min.Bottom, max.Bottom)
                          );
    }

    /// <summary>
    /// Expands this Pixel4 as a rectangle by the specified <paramref name="amount"/><br/>
    /// </summary>
    /// <returns></returns>
    public static Pixel4 Inflate(Pixel4 value, Pixel4 amount, TransformOrigin origin)
    {
        return origin switch
        {
            TransformOrigin.TopLeft => new Pixel4(value.Left, value.Top, value.Right + amount.Right + amount.Left, value.Bottom + amount.Bottom + amount.Top),
            TransformOrigin.Center => new Pixel4(value.Left - amount.Left, value.Top - amount.Top, value.Right + amount.Right, value.Bottom + amount.Bottom),
            _ => throw new NotImplementedException()
        };
    }

    /// <summary>
    /// Expands this Pixel4 as a rectangle by the specified <paramref name="amount"/><br/>
    /// </summary>
    /// <returns></returns>
    public static Pixel4 Deflate(Pixel4 value, Pixel4 amount, TransformOrigin origin)
    {
        return origin switch
        {
            TransformOrigin.TopLeft => new Pixel4(value.Left, value.Top, value.Right - amount.Right - amount.Left, value.Bottom - amount.Bottom - amount.Top),
            TransformOrigin.Center => new Pixel4(value.Left + amount.Left, value.Top + amount.Top, value.Right - amount.Right, value.Bottom - amount.Bottom),
            _ => throw new NotImplementedException()
        };
    }

    internal Rectangle AsRectangle()
        => new Rectangle(Left, Top, Left+Right, Top+Bottom);
}
