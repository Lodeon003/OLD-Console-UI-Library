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
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using System.Xml;

namespace Lodeon.Terminal.UI;

public interface IElement : ITransform, IRenderable
{
    public IContainer? Parent { get; set; }
    public Page? Page { get; }

    public event EventHandler? ParentChanged;
    
    /// <summary>
    /// Event Raised whenever this element or it's children (if container) request to be drawn<br/>
    /// 'sender' is the element that requested to be drawn
    /// </summary>
    public event EventHandler? DrawRequested;

    public static Page FromXML(string path, Page.InitializationContext pageContext)
    {
        XmlDocument xml = new XmlDocument();
        xml.Load(path);

        Page page = new Page(pageContext);
        return page;
    }
}

/// <summary>
/// Base class for UI elements 
/// </summary>
public abstract class Element<TContext> : IElement where TContext : IElement.InitializationContext
{
    public Element(TContext context)
    {
        Page = context.Page;
        Parent = context.Parent;
        
        SizeChanged += OnSizeChanged;
        
        _buffer = new GraphicBuffer(context.InitialSize.X, context.InitialSize.Y);
        _canvas = new GraphicCanvas(_buffer);
    }

    public IContainer? Parent { get => GetParent(); set => SetParent(value); }
    public PixelPoint Position { get; set; } = default;
    public PixelPoint Size { get; set; } = default;
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
    /// <summary>
    /// Invoked whenever this element requested to be drawn
    /// </summary>
    /// <param name="canvas">The object used to draw this element's graphics</param>
    /// <param name="width">The width of this element in terminal window characters</param>
    /// <param name="height">The height of this element in terminal window characters</param>
    protected abstract void OnDraw(GraphicCanvas canvas, int width, int height);
    
    /// <summary>
    /// Invoked whenever this element or it's children (if container) request to be drawn
    /// </summary>
    /// <param name="sender">The element that requested to be drawn</param>
    protected virtual void OnDrawRequested(IElement sender) { }

    /// <summary>
    /// Invoked whenever a key was pressed on the keyboard
    /// </summary>
    /// <param name="info">Information about the key being pressed</param>
    protected virtual void OnKeyDown(ConsoleKeyInfo info) { }
    
    // ------   PUBLIC     ------------------------------------------------------------------------------------------

    public ReadOnlySpan<Pixel> GetGraphics()
        => _buffer.GetGraphics();

    public Rectangle GetScreenArea()
        => _buffer.GetArea().Move(Position);

    public PixelPoint GetPosition()
        => Position;

    public PixelPoint GetSize()
        => Size;

    public void SetParent(IContainer? value)
    {
        // NOTE: it can allow NULL values. It is made so element can be detached
        // from containers and be left outside of the visual tree
        if (_parent == value)
            return;
            
        // Unhook events from old page
        Page?.OnKeyDown -= Page_KeyDownHandler;

        // Update values
        _parent = value;
        Page = _parent?.Page;

        // Hook events to new page if not null
        Page?.OnKeyDown += Page_KeyDownHandler;

        // Notify
        ParentChanged?.Invoke(this, EventArgs.Empty);
    }

    public IContainer GetParent()
        => _parent;

    /// <summary>
    /// Calls element's draw procedure and signals page to draw this element.<br/>
    /// (Invokes <see cref="OnDraw(GraphicCanvas, int, int)"/> method and raises <see cref="DrawRequested"/> event that page will listen to)
    /// </summary>
    protected void Draw()
    {
        OnDrawRequested(this);
        OnDraw(_canvas, _buffer.Width, _buffer.Height);
        DrawRequested?.Invoke(this, EventArgs.Empty);
    }

    // ------   PRIVATE     ------------------------------------------------------------------------------------------

    private void OnSizeChanged(ITransform _, PixelPoint oldSize, PixelPoint newSize)
    {
        _buffer.Resize(newSize.X, newSize.Y);
        Draw();
    }

    /// <summary>
    /// Instantiates a new element from an XML code snippet, setting <paramref cref="context"/> parameters form XML<br/>
    /// <b>Note: </b> <paramref cref="context"/> parameters set in XML will overwrite values passed as method parameters
    /// </summary>
    public static TElement FromXML<TElement, TContext>(XmlNode node, TContext context) where TElement : Element<TContext>
    {
        TElement.Parameter[] params = context.GetParameters();
        throw new NotImplementedException("Read node attributes and set context variables");
    }

    /// <summary>
    /// Rappresents a reflected property which are adorned with <see cref="ParameterAttribute"/>
    /// </summary>
    public struct Parameter
    {
        PropertyInfo Property { get; init; }
        public ParameterAttribute Attribute { get; init; }
    }

    /// <summary>
    /// Returns all properties contained in this implementation of <see cref="InitializationContext"/> which are adorned with <see cref="ParameterAttribute"/>
    /// </summary>
    public class InitializationContext
    {
        [Parameter("Size")]
        public PixelPoint InitialSize { get; init; } = default;

        public IContainer? Parent { get; init; }
        public Page Page { get; init; } = default!;
        public Navigator<string, Page> Navigator { get; init; } = default!;

        /// <summary>
        /// Returns all properties contained in this implementation of <see cref="InitializationContext"/> which has a <see cref="ParameterAttribute"/>
        /// </summary>
        public static Parameter[] GetParameters()
        {
            List<Parameter> params = new();
            foreach (PropertyInfo prop in this.GetType().GetProperties())
            {
                ParameterAttribute attribute = prop.GetCustomAttribute<ParameterAttribute>();

                if(attribute is null)
                    continue;

                params.Add(new() {Property = prop, Attribute = attribute});
            }
            return params.ToArray();
        }
    }

    /// <summary>
    /// Attribute used on <see cref="InitializationContext"/> properties to allow custom compilers to set them<br/>
    /// Use <see cref="InitializationContext.GetParameters"/> to retreive publicly settable properties using this attribute
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ParameterAttribute : System.Attribute
    {
        public ParameterAttribute(string? name = String.Empty)
        {
            Name = name;
        }

        public string? Name { get;  init; }
    }
}

public interface IContainer : IElement
{
    /// <summary>
    /// Adds a child element to this container
    /// </summary>
    void AddChild(IElement element);
    
    /// <summary>
    /// Removes a child element to this container if present
    /// </summary>
    void RemoveChild(IElement element);

    /// <summary>
    /// Returns a container's child or <see cref="null"/> if no child exists with specified index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    IElement? GetChild(int index);
}

public abstract class Container<TContext> : Element<TContext>, IContainer where TContext : Container.InitializationContext
{
    public Container(TContext context) : base(context) { }

    private List<IElement> _children = new();

    public sealed override event EventHandler? DrawRequested;

    public void AddChild(IElement child)
    {
        _children.Add(child);
        OnChildAdded(child);

        child.Parent = this;
        child.ParentChanged += Child_OnParentChanged;
        child.DrawRequested += Child_OnDrawRequested;
    }

    public void RemoveChild(IElement child)
    {
        _children.Remove(child);
        OnChildRemoved(child);

        child.DrawRequested -= Child_OnDrawRequested;
        child.ParentChanged -= Child_OnParentChanged;
        
        if(child.Parent == this)
            child.Parent = null;
    }

    public IElement? GetChild(int index)
    {
        try {
            return _children[index];
        }
        catch { 
            return null;
        }
    }

    protected virtual void OnChildAdded(IElement child) { }
    protected virtual void OnChildRemoved(IElement child) { }

    /// <summary>
    /// Invoked if children's parent was changed and this was his last parent.<br/>
    /// Element must be removed as it's not a child anymore. (Won't fire if you call <see cref="RemoveChild(IElement)"/>)
    /// </summary>
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
    {
        if (sender is not IElement element)
            throw new ArgumentNullException();

        OnDrawRequested(element);
        DrawRequested?.Invoke(sender, e);
    }

    private void UpdateLayout()
    {
        Span<IElement> children = CollectionsMarhsal.AsSpan(_children);
        Span<ITransform> transforms;

        // Allocate array
        try { transforms = stackalloc ITransform[children.Length]; }
        catch { transforms = ArrayPool<ITransform>.Shared.Rent(children.Length); }

        try { transforms = stackalloc ITransform[children.Length]; }
        catch { transforms = ArrayPool<ITransform>.Shared.Rent(children.Length); }

        Pixel4 area = transforms.GetArea();

        for(int i = 0; i < children.Length; i++)
            OnLayout(area, transforms, i);
    }

    protected virtual Rectangle OnLayout(Rectangle area, ReadOnlySpan<ITransform> elements, int index)
    {
        throw new NotImplementedException("BOH");
    }

    public class InitializationContext : IElement.InitializationContext
    {
        // Type of layout handler    
    }
}

public abstract class Container : Container<Container.InitializationContext>
{
    protected Container(Container.InitializationContext context) : base(context)
    {
    }
}