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
    public class Context
    {
        public delegate void ParentChangeHandler(IElement? oldParent, IElement? newParent);
        public event ParentChangeHandler? ParentChanged;

        private IElement? _parent;
        public IElement? Parent { get => _parent; set { IElement? oldParent = _parent; _parent = value; ParentChanged?.Invoke(oldParent, value); } }
        
        public Page Page { get; init; } = default!;
        public Navigator<string, Page> Navigator { get; init; } = default!;

        public PixelPoint initialSize { get; init; }
    }
}

/// <summary>
/// Base class for UI elements 
/// </summary>
public abstract class Element<TContext> : IElement where TContext : IElement.Context
{
    //public delegate void DisplayRequestedDel(ReadonlyGraphicBuffer graphic);

    public Element(TContext context)
    {
        Context = context;
        SizeChanged += SizeChangedHandler;
        _buffer = new GraphicBuffer(Context.initialSize.X, context.initialSize.Y);
        _canvas = new GraphicCanvas(_buffer);
    }

    protected TContext Context { get; private init; }

    public PixelPoint Position { get; set; } = default;
    public PixelPoint Size { get; set; } = default;

    private GraphicBuffer _buffer;
    private GraphicCanvas _canvas;

    protected void Display()
        => Context.Page.Display(this);

    /// <see cref="IRenderable"/> Implementation
    public abstract ReadOnlySpan<Pixel> GetGraphics();
    public abstract Rectangle GetScreenArea();


    /// <see cref="ITransform"/> Implementation
    public abstract PixelPoint GetPosition();
    public abstract PixelPoint GetSize();
    public event TransformChangedEvent? PositionChanged;
    public event TransformChangedEvent? SizeChanged;

    private void SizeChangedHandler(TransformChangeArgs<PixelPoint> args)
        => OnDraw(_canvas, _buffer.Width, _buffer.Height);

    protected abstract void OnDraw(GraphicCanvas canvas, int width, int height);
}

public interface IContainer
{
    public class Context : IElement.Context
    {
        // Type of layout handler    
    }
}

public abstract class Container<TContext> : Element<TContext>, IContainer where TContext : IContainer.Context
{
    protected Container(TContext context) : base(context) { }

    private List<IElement> _children = new();

    public void AddChild(IElement child)
        => _children.Add(child);

    public void RemoveChild(IElement child)
        => _children.Remove(child);

    public IElement GetChild(int index)
        => _children[index];

    private void UpdateLayout()
    {
        throw new NotImplementedException("Update children's position");
    }
}