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
    private Page? _page;

    public ITransform Parent { get { if (_parent == null) throw new Exception("Internal error. Element was used before initializing using \"Initialize\" function"); return _parent; } }
    private ITransform? _parent;

    protected Navigator<string, Page> Navigator { get { if (_navigator is null) throw new ArgumentNullException(nameof(Navigator), "Element was not initialized"); return _navigator; } }
    private Navigator<string, Page>? _navigator;

    protected ExceptionHandler ExceptionHandler { get { if (_exceptionHandler is null) throw new ArgumentNullException(nameof(ExceptionHandler), "Element was not initialized"); return _exceptionHandler; } }
    private ExceptionHandler? _exceptionHandler;
    //public Element[] Children { get { if (_children == null) throw new Exception("Internal error. Element was used before initializing using \"InitChildren\" function"); return _children; } }

    public abstract bool IsFocusable { get; }
    public abstract bool IsContainer { get; }
    

    public delegate void DisplayRequestedDel(ReadonlyGraphicBuffer graphic);
    public event DisplayRequestedDel? DisplayRequested;
    public event TransformChangedEvent? PositionChanged;
    public event TransformChangedEvent? SizeChanged;

    public virtual void Initialize(Page page, ITransform parent, ExceptionHandler exceptionHandler, Navigator<string, Page> navigator)
    {
        _page = page;
        ArgumentNullException.ThrowIfNull(_page);

        _parent = parent;
        ArgumentNullException.ThrowIfNull(_parent);

        _exceptionHandler = exceptionHandler;
        ArgumentNullException.ThrowIfNull(_exceptionHandler);

        _navigator = navigator;
        ArgumentNullException.ThrowIfNull(_navigator);
    }

    protected void Display()
        => Page.Display(this);

    public abstract ReadOnlySpan<Pixel> GetGraphics();

    public abstract Rectangle GetScreenArea();

    public abstract PixelPoint GetPosition();

    public abstract PixelPoint GetSize();

    internal abstract ReadOnlySpan<Element> GetChildren();
    public abstract void AddChild(Element element);
    public abstract void RemoveChild(Element element);
}
