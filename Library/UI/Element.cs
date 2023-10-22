using Lodeon.Terminal.Graphics;
using Lodeon.Terminal.UI.Layout;
using Lodeon.Terminal.UI.Navigation;
using Lodeon.Terminal.UI.Paging;
////using Lodeon.Terminal.UI.Layout;
using Lodeon.Terminal.UI.Units;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Lodeon.Terminal.UI;

public interface IElement : ITransform, IRenderable
{
    public IContainer? Parent { get; set; }
    public Page? Page { get; protected set; }

    public event EventHandler? ParentChanged;
    
    /// <summary>
    /// Event Raised whenever this element or it's children (if container) request to be drawn<br/>
    /// 'sender' is the element that requested to be drawn
    /// </summary>
    public event EventHandler? DrawRequested;

    public class Context
    {
        public delegate void ParentChangeHandler(IContainer? oldParent, IContainer? newParent);
        public event ParentChangeHandler? ParentChanged;

        private IContainer? _parent;
        public IContainer? Parent { get => _parent; set { var oldParent = _parent; _parent = value; ParentChanged?.Invoke(oldParent, value); } }
        
        public Page Page { get; init; } = default!;
        public Navigator<string, Page> Navigator { get; init; } = default!;

        public PixelPoint initialSize { get; set; } = default;
    }
}

/// <summary>
/// Base class for UI elements 
/// </summary>
public abstract class Element<TContext> : IElement where TContext : IElement.Context
{
    public Element(TContext context)
    {
        Page = context.Page;
        Parent = context.Parent;
        
        SizeChanged += OnSizeChanged;
        
        _buffer = new GraphicBuffer(context.initialSize.X, context.initialSize.Y);
        _canvas = new GraphicCanvas(_buffer);
    }

    public PixelPoint Position { get; set; } = default;
    public PixelPoint Size { get; set; } = default;
    public IContainer? Parent { get => _parent; set { if (_parent == value) return; _parent = value; Page = _parent?.Page; ParentChanged?.Invoke(this, EventArgs.Empty); } }
    public Page? Page { get; private set; }

    private GraphicBuffer _buffer;
    private GraphicCanvas _canvas;
    private IContainer? _parent;

    // ------   EVENTS     ------------------------------------------------------------------------------------------
    public event ITransform.PositionChangeDel? PositionChanged;
    public event ITransform.SizeChangeDel? SizeChanged;
    public event ITransform.TransformChangeDel? TransformChanged;
    public virtual event EventHandler? DrawRequested;
    public event EventHandler? ParentChanged;

    // ------   OVERRIDABLE     ------------------------------------------------------------------------------------------
    protected abstract void OnDraw(GraphicCanvas canvas, int width, int height);

    // ------   PUBLIC     ------------------------------------------------------------------------------------------

    public ReadOnlySpan<Pixel> GetGraphics()
        => _buffer.GetGraphics();

    public Rectangle GetScreenArea()
        => _buffer.GetArea().Move(Position);

    public PixelPoint GetPosition()
        => Position;

    public PixelPoint GetSize()
        => Size;

    /// <summary>
    /// Calls element's draw procedure and signals page to draw this element.<br/>
    /// (Invokes <see cref="OnDraw(GraphicCanvas, int, int)"/> method and raises <see cref="DrawRequested"/> event that page will listen to)
    /// </summary>
    protected void Draw()
    {
        OnDraw(_canvas, _buffer.Width, _buffer.Height);
        DrawRequested?.Invoke(this, null!);
    }

    // ------   PRIVATE     ------------------------------------------------------------------------------------------

    private void OnSizeChanged(ITransform _, PixelPoint oldSize, PixelPoint newSize)
    {
        _buffer.Resize(newSize.X, newSize.Y);
        Draw();
    }
}

public interface IContainer : IElement
{
    void AddChild(IElement element);
    void RemoveChild(IElement element);
    IElement GetChild(int index);

    public class Context : IElement.Context
    {
        // Type of layout handler    
    }
}

public abstract class Container<TContext> : Element<TContext>, IContainer where TContext : IContainer.Context
{
    protected Container(TContext context) : base(context) { }

    private List<IElement> _children = new();

    public sealed override event EventHandler? DrawRequested;

    public void AddChild(IElement child)
    {
        _children.Add(child);

        child.Parent = this;
        child.ParentChanged += Child_OnParentChanged;
        child.DrawRequested += Child_OnDrawRequested;
    }

    public void RemoveChild(IElement child)
    {
        _children.Remove(child);

        child.DrawRequested -= Child_OnDrawRequested;
        child.ParentChanged -= Child_OnParentChanged;
        
        if(child.Parent == this)
            child.Parent = null;
    }

    public IElement GetChild(int index)
        => _children[index];


    /// <summary>
    /// Invoked if children's parent was changed and this was his last parent.<br/>
    /// Element must be removed as it's not a child anymore. (Won't fire if you call <see cref="RemoveChild(IElement)"/>)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="ArgumentNullException"></exception>
    private void Child_OnParentChanged(object? sender, EventArgs e)
    {
        // If the parent was changed from outside of this parent element, remove it from it's children
        IElement? child = sender as IElement;

        if (child is null)
            throw new ArgumentNullException();

        RemoveChild(child);
    }

    ///<summary>
    /// Invoked when a child requested to be drawn. Forward the request so this' parent can bring it to the page (upmost parent)
    /// </summary>
    private void Child_OnDrawRequested(object? sender, EventArgs e)
        => DrawRequested?.Invoke(sender, e);

    private void UpdateLayout()
    {
        throw new NotImplementedException("Update children's position");
    }
}