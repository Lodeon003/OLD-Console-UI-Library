using Lodeon.Terminal.UI.Layout;
using Lodeon.Terminal.UI.Units;
using System.Drawing;

namespace Lodeon.Terminal.UI
{
    public class RootElement : Element
    {
        public RootElement() {}

        private Element[]? _children;

        public override bool IsFocusable => false;
        public override bool IsContainer => true;

        public override void AddChild(Element element)
        {
            throw new NotImplementedException();
        }

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

        public override void RemoveChild(Element element)
        {
            throw new NotImplementedException();
        }

        internal override ReadOnlySpan<Element> GetChildren()
            => _children;

        internal void SetChildren(Element[] elements)
        {
            _children = elements;
        }
    }
}