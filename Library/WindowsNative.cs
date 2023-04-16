using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lodeon.Terminal;

internal static class WindowsNative
{
    [DllImport("Kernel32")]
    public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("Kernel32")]
    public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [DllImport("Kernel32", SetLastError = true)]
    public static extern IntPtr GetStdHandle(int nStdHandle);

    public const int STD_OUTPUT_HANDLE = -11;
    public const uint ENABLE_PROCESSED_OUTPUT = 0x0001;
    public const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
}