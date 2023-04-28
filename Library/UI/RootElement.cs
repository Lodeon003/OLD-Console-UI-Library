using Lodeon.Terminal.UI.Layout;
using Lodeon.Terminal.UI.Units;
using System.Drawing;

namespace Lodeon.Terminal.UI
{
    public class RootElement : Element
    {
        private Element[]? _children;

        public RootElement() {}

        public override ReadOnlySpan<Pixel> GetGraphics()
            => Span<Pixel>.Empty;

        public override PixelPoint GetPosition()
            => Page.GetPosition();

        public override Rectangle GetScreenArea()
        {
            PixelPoint pos = Parent.GetPosition();
            PixelPoint size = Parent.GetSize();
            
            return new Rectangle(pos.X, pos.Y, size.X, size.Y);
        }

        public override PixelPoint GetSize()
            => Page.GetSize();

        internal override ReadOnlySpan<Element> GetChildren()
            => _children;

        internal void SetChildren(Element[] elements)
        {
            _children = elements;
        }
    }
}