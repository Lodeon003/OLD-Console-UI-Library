using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lodeon.Terminal;

public class Debugger
{
    public static event EventHandler<string>? OnLog;
    public static void Log(string message)
    {
        OnLog?.Invoke(null, message);
    }
}
