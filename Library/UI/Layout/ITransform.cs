using Lodeon.Terminal.UI.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lodeon.Terminal.UI.Layout;

public delegate void TransformChangedEvent(TransformChangeArgs<PixelPoint> args);

public interface ITransform
{
    public event TransformChangedEvent PositionChanged;
    public event TransformChangedEvent SizeChanged;
    public PixelPoint GetPosition();
    public PixelPoint GetSize();
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