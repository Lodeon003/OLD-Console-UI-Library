using Lodeon.Terminal.Graphics;
using Lodeon.Terminal.UI;
using Lodeon.Terminal.UI.Units;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;
using Rectangle = System.Drawing.Rectangle;

namespace Lodeon.Terminal.UI.Layout;

public abstract class LayoutElement : Element
{
    public LayoutElement()
    {
    }

    protected GraphicCanvas Canvas { get { if (_outCanvas == null) throw new Exception("Internal error. Element was used before initializing using \"Initialize\" function"); return _outCanvas; } }
    protected ReadonlyGraphicBuffer CanvasView { get { if (_canvasView == null) throw new Exception("Internal error. Element was used before initializing using \"Initialize\" function"); return _canvasView; } }

    internal sealed override ReadOnlySpan<Element> GetChildren()
        => _children;

    public sealed override ReadOnlySpan<Pixel> GetGraphics()
        => _buffer.GetGraphics();

    public sealed override Rectangle GetScreenArea()
        => _buffer.GetScreenArea();

    public sealed override void Initialize(Page page, ITransform parent, ExceptionHandler handler, Navigator<string, Page> navigator)
    {
        base.Initialize(page, parent, handler, navigator);

        _buffer = new GraphicBuffer();
        _outCanvas = new GraphicCanvas(_buffer);
        _canvasView = _outCanvas.AsReadonly();

        PropertyChanged += LayoutPropertyChangedCallback;
    }

    //protected GraphicBuffer Buffer { get { if (_buffer == null) throw new Exception("Internal error. Element was used before initializing using \"Initialize\" function"); return _buffer; } }
    private GraphicBuffer? _buffer;
    private GraphicCanvas _outCanvas;
    private LayoutResult _currentLayout;
    private LayoutElement[] _children;
    private ReadonlyGraphicBuffer? _canvasView;

    //public abstract LayoutResult[] GetResultArray();
    //public abstract LayoutResult[] GetParentResultArray();
    protected abstract void OnResize(GraphicCanvas screenBuffer, Rectangle screenArea);

    #region Events
    // Delegates
    public delegate void LayoutElementArgs(LayoutElement element);
    public delegate void LayoutPointArgs(LayoutElement element, Point point);
    public delegate void LayoutPoint4Args(LayoutElement element, Point4 point);


    // ITransform Events
    public event TransformChangedEvent? PositionChanged;
    public event TransformChangedEvent? SizeChanged;

    // Default Events
    public event LayoutElementArgs? PropertyChanged;
    public event LayoutPoint4Args? MarginChanged;
    public event LayoutPoint4Args? BorderChanged;
    public event LayoutPoint4Args? PaddingChanged;
    #endregion
    
    #region Layout Properties
    public LayoutPosition PositionKind { get; set; }

    private Point _position;
    /// <summary>
    /// The position of this element relative to it's parent
    /// </summary>
    public Point Position
    {
        get => _position;
        set
        {
            if (value == _position)
                return;

            _position = value;
            PropertyChanged?.Invoke(this);
        }
    }

    private PixelPoint _minPosition;
    public PixelPoint MinPosition
    {
        get => _minPosition;
        set { _minPosition = value; PropertyChanged?.Invoke(this); PositionChanged?.Invoke(new(this, _minPosition, value)); }
    }

    private PixelPoint _maxPosition;
    public PixelPoint MaxPosition
    {
        get => _maxPosition;
        set { _maxPosition = value; PropertyChanged?.Invoke(this); PositionChanged?.Invoke(new(this, _maxPosition, value)); }
    }
     
    private Point4 _padding = Point4.Zero;
    public Point4 Padding
    {
        get => _padding;
        set { _padding = value; PropertyChanged?.Invoke(this); PaddingChanged?.Invoke(this, value); }
    }

    private Pixel4 _minPadding = Pixel4.Zero;
    public Pixel4 MinPadding
    {
        get => _minPadding;
        set { _minPadding = value; PropertyChanged?.Invoke(this); PaddingChanged?.Invoke(this, Padding); } // change
    }

    private Pixel4 _maxPadding = Pixel4.Max;
    public Pixel4 MaxPadding
    {
        get => _maxPadding;
        set { _maxPadding = value; PropertyChanged?.Invoke(this); PaddingChanged?.Invoke(this, Padding); }
    }  
    
    private Point4 _margin = Point4.Zero;
    public Point4 Margin
    {
        get => _margin;
        set { _margin = value; PropertyChanged?.Invoke(this); MarginChanged?.Invoke(this, value); }
    }

    private Pixel4 _maxMargin = Pixel4.Max;
    public Pixel4 MaxMargin
    {
        get => _maxMargin;
        set { _maxMargin = value; PropertyChanged?.Invoke(this); MarginChanged?.Invoke(this, Margin); }
    }

    private Pixel4 _minMargin = Pixel4.Zero;
    public Pixel4 MinMargin
    {
        get => _minMargin;
        set { _minMargin = value; PropertyChanged?.Invoke(this); MarginChanged?.Invoke(this, Margin); }
    }

    private Point _size = Point.One;
    public Point Size
    {
        get => _size;
        set { _size = value; PropertyChanged?.Invoke(this); SizeChanged?.Invoke(new()); }
    }

    private PixelPoint _minSize = PixelPoint.Zero;
    public PixelPoint MinSize
    {
        get => _minSize;
        set { _minSize = value; PropertyChanged?.Invoke(this); } // change
    }

    private PixelPoint _maxSize = PixelPoint.Max;
    public PixelPoint MaxSize
    {
        get => _maxSize;
        set { _maxSize = value; PropertyChanged?.Invoke(this); }
    }
    #endregion

    #region Old Compile
    //public LayoutResult Compile(ILayoutTransform root)
    //{
    //    PixelPoint position = root.GetPixelPosition();
    //    PixelPoint size = root.GetPixelSize();
    //
    //    // Find layout for root element. (page or any container)
    //    LayoutResult rootLayout = new LayoutResult(new(position.X, position.Y, size.X, size.Y),
    //                                               new(position.X, position.Y, size.X, size.Y),
    //                                               new(position.X, position.Y, size.X, size.Y),
    //                                               root.GetResultChildArray(), root.GetPositionKind());
    //    
    //    LayoutStack rootStack = new LayoutStack(position, size);
    //    return this.Compile(rootLayout, rootStack);
    //}

    //public LayoutResult Compile()
    //{
    //    PixelPoint position = Parent.GetPosition();
    //    PixelPoint size = Parent.GetSize();
    //
    //    LayoutStack rootStack = new LayoutStack(position, size);
    //
    //    LayoutResult rootLayout = new LayoutResult(new(position.X, position.Y, size.X, size.Y),
    //                                               new(position.X, position.Y, size.X, size.Y),
    //                                               new(position.X, position.Y, size.X, size.Y),
    //                                               null, LayoutPosition.Absolute);
    //
    //    return this.Compile(rootLayout, rootStack);
    //}
    //
    //private LayoutResult Compile(LayoutResult parentLayout, LayoutStack parentStack)
    //{
    //    LayoutResult[] childResults = GetResultArray();
    //    ReadOnlySpan<LayoutElement> children = GetChildren();
    //
    //    LayoutResult thisLayout = CalculateLayout(parentLayout, parentStack, childResults);
    //    LayoutStack thisStack = new LayoutStack(parentLayout.ContentArea.TopLeft, parentLayout.ContentArea.RectSize);
    //
    //    for (int i = 0; i < children.Length; i++)
    //        childResults[i] = children[i].Compile(thisLayout, thisStack);
    //
    //    return thisLayout;
    //}
    #endregion

    /// <summary>
    /// Calculates the element's layout converting it's properties to pixel units
    /// </summary>
    /// <param name="parentLayout"></param>
    /// <param name="childResults"></param>
    /// <returns>A <see cref="LayoutResult"/> object rapresenting the element's occupied space relative to the root parent</returns>
    private void CalculateLayout(LayoutResult parentLayout, LayoutStack parentStack)
    {
        // NOTE: child collection is not yet initialized but it is passed for LayoutResult reference

        PixelPoint contentSize = parentLayout.ContentArea.RectSize;

        // Sizes are compiled keeping pixel values and calculating percentage values of object's available space (parent's ContentArea)
        PixelPoint size = PixelPoint.Clamp(PixelPoint.FromPoint(Size, contentSize), MinSize, MaxSize);
        Pixel4 margin = Pixel4.Clamp(Pixel4.FromPoint(Margin, contentSize), MinMargin, MaxMargin);
        Pixel4 padding = Pixel4.Clamp(Pixel4.FromPoint(Padding, contentSize), MinPadding, MaxPadding);

        // Impelent struct algorithm
        //PixelPoint position = PositionKind switch
        //{
        //    LayoutPosition.Stack => parentStack.Add(size, margin),
        //    LayoutPosition.Absolute => PixelPoint.Clamp(PixelPoint.FromPoint(Position, contentSize), MinPosition, MaxPosition),
        //    _ => throw new NotImplementedException($"Only {LayoutPosition.Stack} and {LayoutPosition.Absolute} have been defined")
        //};
        throw new NotImplementedException();
        Pixel4 startArea, totalArea, actualArea, contentArea;

        //startArea = new Pixel4(position, size);
        totalArea = Pixel4.Inflate(startArea, margin, TransformOrigin.TopLeft);
        actualArea = Pixel4.Deflate(totalArea, margin, TransformOrigin.Center);
        contentArea = Pixel4.Deflate(actualArea, padding, TransformOrigin.Center);

        /* Start erea      total area                        actual area
         * (top left)      (shifted to maintain Position)    (from middle of total)
         * ●◾◾           ●◽◽◽◽◽◽                        ●
         * ◾◾◾          ◽◽◾◾◾◽◽                           ◾◾◾
         * ◾◾◾          ◽◽◾◾◾◽◽                           ◾◾◾
         *                ◽◽◾◾◾◽◽                           ◾◾◾
         *                ◽◽◽◽◽◽◽
         */


        _currentLayout = new LayoutResult(totalArea, actualArea, contentArea, PositionKind);
    }

    private void LayoutPropertyChangedCallback(LayoutElement @this)
        => Update();

    public sealed override PixelPoint GetPosition()
        => _currentLayout.Position;

    public sealed override PixelPoint GetSize()
        => _currentLayout.ContentArea.RectSize;

    /// <summary>
    /// To test. works but wrong (read inside)
    /// </summary>
    private void Update()
    {
        LayoutResult parentLayout;
        ReadOnlySpan<LayoutElement> elements;

        if(Parent is not LayoutElement parent)
        {
        PixelPoint parentPos = Parent.GetPosition();
        PixelPoint parentSize = Parent.GetSize();

            Pixel4 area = new Pixel4(parentPos, parentSize);
            parentLayout = new LayoutResult(area, area, area, LayoutPosition.Absolute);

            LayoutElement @this = this;
            elements = MemoryMarshal.CreateSpan(ref @this, 1);
        }
        else
        {
            parentLayout = parent._currentLayout;
            elements = parent._children;
        }

        throw new NotImplementedException("Stacking algorithm should depend on parent's enum");
        LayoutStack parentStack = new LayoutStack(parentLayout.Position, parentLayout.ContentArea.RectSize, LayoutStack.HorizontalAlign.Left, LayoutStack.VerticalAlign.Top, LayoutStack.Orientation.Horizontal);
        
        // Calculate this element's layout (and siblings' layout if there are)
        for (int i = 0; i < elements.Length; i++)
        {
            elements[i].CalculateLayout(parentLayout, parentStack);
            elements[i]._buffer.Resize(_currentLayout.ActualArea.RectSize.X, _currentLayout.ActualArea.RectSize.Y);
            elements[i].OnResize(_outCanvas, _currentLayout.ActualArea.AsRectangle());
        }

        // Update children layout completely
        // [!] Surely wrong, when a children gets updated it updates other children as well, causing them to be recalculated lot of times each
        for (int i = 0; i < _children.Length; i++)
            _children[i].Update();
    }

    internal void SetChildren(LayoutElement[] children)
    {
        _children = children;
    }

    public static RootElement? TreeFromXml(string path, Page page, ExceptionHandler handler, Navigator<string, Page> navigator)
    {
        try
        {
            XmlDocument document = new XmlDocument();
            document.Load(path);
            
            RootElement root = new RootElement();
            root.Initialize(page, page, handler, navigator);

            LayoutElement[] elements = document.ChildNodes.Count == 0 ? Array.Empty<LayoutElement>() : new LayoutElement[document.ChildNodes.Count];

            for (int i = 0; i < document.ChildNodes.Count; i++)
                elements[i] = FromNode(document.ChildNodes.Item(i), page, root, handler, navigator);

            root.SetChildren(elements);
            return root;
        }
        catch(Exception e)
        {
            handler.Throw(e);
            return null;
        }

    }
    private static LayoutElement FromNode(XmlNode node, UI.Page page, ITransform parent, ExceptionHandler handler, Navigator<string, Page> navigator)
    {
        LayoutElement element = (LayoutElement)ElementCache.Instance.Instantiate(node.Name);
        element.Initialize(page, parent, handler, navigator);
        
        LayoutElement[] children = node.ChildNodes.Count == 0 ? Array.Empty<LayoutElement>() : new LayoutElement[node.ChildNodes.Count];

        for (int i = 0; i < node.ChildNodes.Count; i++)
            children[i] = FromNode(node.ChildNodes.Item(i), page, element, handler, navigator);


        // Get all properties in element type
        foreach (PropertyInfo prop in element.GetType().GetProperties().Where((x) => x.GetSetMethod(true) != null && x.GetGetMethod(true) != null))
        {
            XmlAttribute? attribute = node.Attributes?[prop.Name];
        
            // If property has been specified in XML
            if (attribute == null || !prop.DeclaringType.IsAssignableFrom(typeof(IStringConvertible)))
                continue;

            // [!] Convert value to real type instead of string before converting
            prop.SetValue(element, node.Value);
        }
        
        if(element.IsContainer)
            element.SetChildren(children);

        return element;
    }

    private static Type[] StringTypeArray { get; } = new Type[] { typeof(string) };
    //private penis penis penis penis penis penis penis penis penis vagina porn fuck fuck homosexual sex balls and cock cum cum cum mhhhhhhhhhhhh
}
