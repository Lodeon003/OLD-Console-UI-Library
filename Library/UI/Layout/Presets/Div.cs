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
    public class Square : LayoutElement
    {
        public Square(LayoutElement[] children) : base(children)
        {
        }

        public Color Background { get; set; }

        protected override void OnResize(GraphicCanvas screenBuffer, Rectangle screenArea)
        => screenBuffer.DrawRectangle(screenArea, Background);
    }
}
