namespace Lodeon.Terminal.UI.Units;
public readonly struct Unit
{
    public int Value { get; }
    public UnitKind Kind { get; }

    public static Unit Zero => new Unit(0, UnitKind.Pixel);

    public Unit(int value, UnitKind kind)
    {
        if (kind == UnitKind.Percentage && value < 0 || value > 100)
            throw new ArgumentException("A point rapresented by percentage units can't have coordinates below 0(%) over and 100(%)", nameof(value));

        Value = value;
        Kind = kind;
    }

    public static bool operator ==(Unit left, Unit right)
        => left.Value == right.Value && left.Kind == right.Kind;

    public static bool operator !=(Unit left, Unit right)
        => !(left == right);

    // Caccamolle
}