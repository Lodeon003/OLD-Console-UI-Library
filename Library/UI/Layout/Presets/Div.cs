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

        public override bool IsFocusable => false;

        public override bool IsContainer => false;

        public override void AddChild(Element element) {}
        public override void RemoveChild(Element element) {}

        protected override void OnResize(GraphicCanvas screenBuffer, Rectangle screenArea)
        => screenBuffer.DrawRectangle(screenArea, Background);
    }
}
