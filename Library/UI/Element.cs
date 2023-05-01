using Lodeon.Terminal.Graphics;
using Lodeon.Terminal.UI.Layout;
using Lodeon.Terminal.UI.Units;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Lodeon.Terminal.UI;

/// <summary>
/// Base class for UI elements 
/// </summary>
public abstract class Element : ITransform, IRenderable
{
    protected Page Page { get { if (_page == null) throw new Exception("Internal error. Element was used before initializing using \"Initialize\" function"); return _page; } }
    public ITransform Parent { get { if (_parent == null) throw new Exception("Internal error. Element was used before initializing using \"Initialize\" function"); return _parent; } }
    //public Element[] Children { get { if (_children == null) throw new Exception("Internal error. Element was used before initializing using \"InitChildren\" function"); return _children; } }

    public abstract bool IsFocusable { get; }
    public abstract bool IsContainer { get; }
    
    private Page? _page;
    private ITransform? _parent;

    public delegate void DisplayRequestedDel(ReadonlyGraphicBuffer graphic);
    public event DisplayRequestedDel? DisplayRequested;
    public event TransformChangedEvent? PositionChanged;
    public event TransformChangedEvent? SizeChanged;

    public virtual void Initialize(Page page, ITransform parent)
    {
        _page = page;
        ArgumentNullException.ThrowIfNull(_page);

        _parent = parent;
        ArgumentNullException.ThrowIfNull(_parent);
    }

    protected void Display()
        => Page.Display(this);

    public abstract ReadOnlySpan<Pixel> GetGraphics();

    public abstract Rectangle GetScreenArea();

    public abstract PixelPoint GetPosition();

    public abstract PixelPoint GetSize();

    internal abstract ReadOnlySpan<Element> GetChildren();
}
