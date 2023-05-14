using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lodeon.Terminal.Graphics.Drivers;

public class DriverException : Exception
{
    public DriverException(string message) : base(message)
    {

    }
}
