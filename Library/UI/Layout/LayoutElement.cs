using Lodeon.Terminal.UI;
using Lodeon.Terminal.UI.Units;
using System.Data;
using System.Diagnostics;
using Rectangle = System.Drawing.Rectangle;

namespace Lodeon.Terminal.UI.Layout;

public abstract class LayoutElement : Element
{
    public LayoutElement(LayoutElement[] children)
    {
        ArgumentNullException.ThrowIfNull(children);
        _childrenResults = new LayoutResult[children.Length];
        _children = children;

        _outBuffer = new GraphicBuffer();

        PropertyChanged += OnPropertyChanged;
    }

    private readonly GraphicBuffer _outBuffer;
    private LayoutResult[] _childrenResults;
    private LayoutElement[] _children;

    public abstract LayoutResult[] GetResultArray();
    public abstract LayoutResult[] GetParentResultArray();
    protected abstract void OnResize(GraphicBuffer screenBuffer, Pixel4 screenArea);

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


    public ReadOnlySpan<LayoutElement> GetChildren()
        => _children;

    public LayoutResult Compile()
    {
        PixelPoint position = Parent.GetPosition();
        PixelPoint size = Parent.GetSize();

        LayoutStack rootStack = new LayoutStack(position, size);

        LayoutResult rootLayout = new LayoutResult(new(position.X, position.Y, size.X, size.Y),
                                                   new(position.X, position.Y, size.X, size.Y),
                                                   new(position.X, position.Y, size.X, size.Y),
                                                   null, LayoutPosition.Absolute);

        return this.Compile(rootLayout, rootStack);
    }

    private LayoutResult Compile(LayoutResult parentLayout, LayoutStack parentStack)
    {
        LayoutResult[] childResults = GetResultArray();
        ReadOnlySpan<LayoutElement> children = GetChildren();

        LayoutResult thisLayout = GetLayout(parentLayout, parentStack, childResults);
        LayoutStack thisStack = new LayoutStack(parentLayout.ContentArea.TopLeft, parentLayout.ContentArea.RectSize);

        for (int i = 0; i < children.Length; i++)
            childResults[i] = children[i].Compile(thisLayout, thisStack);

        return thisLayout;
    }

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
    #endregion

    /// <summary>
    /// Calculates the element's layout converting it's properties to pixel units
    /// </summary>
    /// <param name="parentLayout"></param>
    /// <param name="childResults"></param>
    /// <returns>A <see cref="LayoutResult"/> object rapresenting the element's occupied space relative to the root parent</returns>
    private LayoutResult GetLayout(LayoutResult parentLayout, LayoutStack parentStack, IReadOnlyCollection<LayoutResult> childResults)
    {
        // NOTE: child collection is not yet initialized but it is passed for LayoutResult reference

        PixelPoint contentSize = parentLayout.ContentArea.RectSize;

        // Sizes are compiled keeping pixel values and calculating percentage values of object's available space (parent's ContentArea)
        PixelPoint size = PixelPoint.Clamp(PixelPoint.FromPoint(Size, contentSize), MinSize, MaxSize);
        Pixel4 margin = Pixel4.Clamp(Pixel4.FromPoint(Margin, contentSize), MinMargin, MaxMargin);
        Pixel4 padding = Pixel4.Clamp(Pixel4.FromPoint(Padding, contentSize), MinPadding, MaxPadding);

        // Impelent struct algorithm
        PixelPoint position = PositionKind switch
        {
            LayoutPosition.Stack => parentStack.Add(size),
            LayoutPosition.Absolute => PixelPoint.Clamp(PixelPoint.FromPoint(Position, contentSize), MinPosition, MaxPosition),
            _ => throw new NotImplementedException($"Only {LayoutPosition.Stack} and {LayoutPosition.Absolute} have been defined")
        };

        Pixel4 startArea, totalArea, actualArea, contentArea;

        startArea = new Pixel4(position, size);
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


        return new LayoutResult(totalArea, actualArea, contentArea, childResults, PositionKind);
    }


    internal static LayoutElement? TreeFromXml(string path)
    {
        throw new NotImplementedException();
    }

    private void OnPropertyChanged(LayoutElement sender)
        => Update();

    private LayoutResult Update()
    {
        // Make it so when property updates the LayoutResult gets saved on the object itself
        // Have an event to children's PropertyChanged event and make it so that when a child's property change,
        // This' layout recalculates, recaculating all of the children 

        if(Parent is not LayoutElement parent)
        {
            Pixel4 area = new Pixel4(Parent.GetPosition(), Parent.GetSize());
            return new LayoutResult(area, area, area, null, LayoutPosition.Absolute);
        }

        LayoutResult parentLayout = parent.Update();
        LayoutResult thisLayout = GetLayout(parentLayout, default, null);

        OnResize(_outBuffer, thisLayout.ActualArea);
        return thisLayout;
    }

    //private penis penis penis penis penis penis penis penis penis vagina porn fuck fuck homosexual sex balls and cock cum cum cum mhhhhhhhhhhhh
}
