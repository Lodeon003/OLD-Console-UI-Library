using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lodeon.Terminal;

public class ExceptionHandler
{
    public delegate void ExceptionDel(Exception e);
    public event ExceptionDel? ExceptionThrown;
    public event ExceptionDel? ExceptionLogged;

    public IReadOnlyCollection<Exception> Exceptions => _exceptions.AsReadOnly();
    private List<Exception> _exceptions = new List<Exception>();
    
    public void Log(Exception e)
    {
        _exceptions.Add(e);
        ExceptionLogged?.Invoke(e);
    }

    public void Throw(Exception e)
    {
        _exceptions.Add(e);
        ExceptionThrown?.Invoke(e);
    }
}
