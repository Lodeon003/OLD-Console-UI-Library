using Lodeon.Terminal;
using Lodeon.Terminal.Graphics;
using Lodeon.Terminal.UI;
using Lodeon.Terminal.UI.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmarks.TreeNavigator
{
    internal abstract class Nav : INavigable
    {
        public Nav(INavigableContainer par, int x, int y, int width, int height, Color color)
        {
            _parent = par;
            _position = new(x, y);
            _buffer = new GraphicBuffer(width, height);
            _buffer.Fill(color);
            _color = color;
            Canvas = new GraphicCanvas(_buffer);
        }

        public void Display(Driver driver, Color color)
        {
            _buffer.Fill(color);
            driver.Display(_buffer.GetGraphics(), _buffer.GetSourceArea(), _position);
        }

        public void Display(Driver driver)
        {
            Display(driver, _color);
        }

        private Color _color;
        private PixelPoint _position;
        private GraphicBuffer _buffer;
        public GraphicCanvas Canvas;
        private INavigableContainer? _parent;

        public INavigableContainer? GetParent()
        {
            return _parent;
        }

        public bool IsLocked()
        {
            return false;
        }
    }

    internal class NavElement : Nav
    {
        public NavElement(INavigableContainer par, int x, int y, int width, int height, Color color) : base(par, x, y, width, height, color)
        {
        }
    }

    internal class NavContainer : Nav, INavigableContainer
    {
        private List<Nav> children = new List<Nav>();

        public NavContainer(INavigableContainer par, int x, int y, int width, int height, Color color) : base(par, x, y, width, height, color)
        {
        }

        public void AddChildren(params Nav[] elements)
        {
            children.AddRange(elements);
        }

        public INavigable[] GetChildren()
        {
            return children.ToArray();
        }
    }
}
