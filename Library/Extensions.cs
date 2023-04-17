using System;
using System.Collections.Generic;
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
    }
}
