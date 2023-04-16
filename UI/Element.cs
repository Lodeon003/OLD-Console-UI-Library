using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lodeon.Terminal.UI;

/// <summary>
/// Base class for UI elements 
/// </summary>
public abstract class Element
{
    protected Driver Out { get { if (_driver is null) throw new ArgumentNullException(this.GetType().FullName, "Element was not initialized"); return _driver; } }
    private Driver? _driver;

    public void Initialize(Driver output)
    {
        ArgumentNullException.ThrowIfNull(output);
        _driver = output;
    }
}
