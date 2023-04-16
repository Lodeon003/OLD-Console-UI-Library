namespace Lodeon.Terminal.UI.Units;

public readonly struct Point4
{
    public Unit Left { get; private init; }
    public Unit Top { get; private init; }
    public Unit Right { get; private init; }
    public Unit Bottom { get; private init; }
    public static Point4 Zero = new Point4(Unit.Zero, Unit.Zero, Unit.Zero, Unit.Zero);

    public Point4(Unit left, Unit top, Unit right, Unit bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    //public bool IsPixel => Left.Kind == UnitKind.Pixel && Top.Kind == UnitKind.Pixel && Right.Kind == UnitKind.Pixel && Bottom.Kind == UnitKind.Pixel
}
