using Lodeon.Terminal.UI.Units;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lodeon.Terminal;

internal static class Extensions
{
    public static async Task WaitAsync(this CancellationToken token)
        => await Task.Run(() => token.WaitHandle.WaitOne());

    public static Rectangle Clamp(this Rectangle rect, Rectangle clamp)
    {
        return new(Math.Clamp(rect.Left, clamp.Left, clamp.Right),
                   Math.Clamp(rect.Top, clamp.Top, clamp.Bottom),
                   Math.Clamp(rect.Right, clamp.Left, clamp.Right),
                   Math.Clamp(rect.Bottom, clamp.Top, clamp.Bottom));
    }

    public static Pixel4 Offset(this Pixel4 rect, PixelPoint offset)
        => new(rect.Left + offset.X, rect.Top + offset.Y, rect.Right + offset.X, rect.Bottom + offset.Y);

    /// <summary>
    /// Converts a span from a type to another based using a rule supplied from the user<br/>
    /// The spans must be same length. If the predicate method throws an exception it gets thrown wrapped inside of a <see cref="InvalidOperationException"/>
    /// </summary>
    /// <exception cref="FormatException"/>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="InvalidOperationException"/>
    public static void Map<TSource, TTarget>(this Span<TSource> source, Span<TTarget> target, Func<TSource, TTarget> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        if(source.Length != target.Length)
            throw new FormatException($"Mapped spans must be of the same length. First span is {source.Length}, second is {target.Length}");

        try
        {
            for(int i = 0; i < source.Length; i++)
               target[i] = predicate.Invoke(source[i]);
        }
        catch(Exception e) {
            throw new InvalidOperationException("The predicate threw an exeption", e);
        }
    }
}
