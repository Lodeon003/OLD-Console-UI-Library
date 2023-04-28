using Lodeon.Terminal.Graphics;
using Lodeon.Terminal.UI.Units;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lodeon.Terminal.UI.Layout.Presets
{
    [Name("Square")]
    public class Square : LayoutElement
    {
        public Color Background { get; set; }

        public override ReadOnlySpan<Pixel> GetGraphics()
        {
            throw new NotImplementedException();
        }

        public override Rectangle GetScreenArea()
        {
            throw new NotImplementedException();
        }

        protected override void OnResize(GraphicCanvas screenBuffer, Rectangle screenArea)
        => screenBuffer.DrawRectangle(screenArea, Background);

        internal override ReadOnlySpan<Element> GetChildren()
        {
            throw new NotImplementedException();
        }
    }
}
