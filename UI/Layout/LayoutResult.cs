using Lodeon.Terminal.UI.Units;

namespace Lodeon.Terminal.UI.Layout;

public struct LayoutResult
{
    public IReadOnlyCollection<LayoutResult>? Children { get; private init; }

    /// <summary>
    /// Rapresents the position of the element (relative to the root element)
    /// </summary>
    public PixelPoint Position { get; private init; }

    /// <summary>
    /// Rapresents the total area occupied by the element (size + margin?)
    /// </summary>
    public Pixel4 TotalArea { get; private init; }

    /// <summary>
    /// Rapresents the area occupied by the element (size)
    /// </summary>
    public Pixel4 ActualArea { get; private init; }

    /// <summary>
    /// Rapresents the area in which child elements will be placed (size - padding)
    /// </summary>
    public Pixel4 ContentArea { get; private init; }
    public LayoutPosition PositionKind { get; private init; }

    public LayoutResult(Pixel4 totalArea, Pixel4 actualArea, Pixel4 contentArea, IReadOnlyCollection<LayoutResult>? children, LayoutPosition positionKind)
    {
        Position = totalArea.TopLeft;
        TotalArea = totalArea;
        ActualArea = actualArea;
        ContentArea = contentArea;
        Children = children;
        PositionKind = positionKind;
        ParentLayout = parentLayout;
    }
}
