using Lodeon.Terminal.UI.Units;

namespace Lodeon.Terminal.UI
{
    internal sealed class RootElement : Element
    {
        private Element[] _children;

        public RootElement() {}

        public override PixelPoint GetPosition()
            => Page.GetPosition();

        public override PixelPoint GetSize()
            => Page.GetSize();

        public void SetChildren(Element[] elements)
            => _children = elements;

        public override ReadOnlySpan<Element> GetChildren()
            => _children;
    }
}