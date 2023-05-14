using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lodeon.Terminal
{
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
    }
}
