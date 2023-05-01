using Lodeon.Terminal.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lodeon.Terminal.UI.Layout.Presets
{
    public class Popup : LayoutElement
    {
        public Popup(string title, string description)
        {
            Title = title;
            Description = description;
        }

        public string Title { get; private init; }
        public string Description { get; private init; }

        public override bool IsFocusable => true;
        public override bool IsContainer => true;

        public override void AddChild(Element element)
        {
            throw new NotImplementedException();
        }

        public override void RemoveChild(Element element)
        {
            throw new NotImplementedException();
        }

        protected override void OnResize(GraphicCanvas screenBuffer, Rectangle screenArea)
        {
            throw new NotImplementedException();
        }
    }
}
