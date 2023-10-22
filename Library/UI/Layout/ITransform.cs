using Lodeon.Terminal.UI.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lodeon.Terminal.UI.Layout;


public interface ITransform
{
    public delegate void PositionChangeDel(ITransform sender, PixelPoint oldPosition, PixelPoint newPosition);
    public delegate void SizeChangeDel(ITransform sender, PixelPoint oldPosition, PixelPoint newPosition);
    public delegate void TransformChangeDel(ITransform sender, Pixel4 oldState, Pixel4 newState);

    public event PositionChangeDel? PositionChanged;
    public event SizeChangeDel? SizeChanged;
    public event TransformChangeDel? TransformChanged;

    public PixelPoint GetPosition();
    public PixelPoint GetSize();
    //public Pixel4 GetArea();
}

public readonly struct TransformChangeArgs<T>
{
    public ITransform Sender { get; private init; }
    public T Value { get; private init; }
    public T OldValue { get; private init; }
    public TransformChangeArgs(ITransform sender, T oldValue, T newValue)
    {
        Sender = sender;
        Value = newValue;
        OldValue = oldValue;
    }
}