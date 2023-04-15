
/*
 1. Stacking, stack horizontally and return to next line if no space available (width, height, totelSize With Borders)
 2. Absolute position. Place items in their absolute position overriding items underneath
 */

using Lodeon.Terminal.UI.Units;

namespace Lodeon.Terminal.UI.Layout;

public interface ILayoutTransform
{
    public ILayoutTransform? GetParent();
    public PixelPoint GetPixelPosition(PixelPoint scale);
    public PixelPoint GetParentPosition();
    public PixelPoint GetPixelSize(PixelPoint scale);
    public PixelPoint GetPixelSize();
    public LayoutPosition GetPositionKind();
    public LayoutResult[] GetResultArray();
}