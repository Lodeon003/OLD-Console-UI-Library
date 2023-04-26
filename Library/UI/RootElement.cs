using Lodeon.Terminal.UI.Units;

namespace Lodeon.Terminal.UI
{
    internal class RootElement : Element
    {
        public RootElement() {}

        public override PixelPoint GetPosition()
            => Page.GetPosition();

        public override PixelPoint GetSize()
            => Page.GetSize();
    }
}