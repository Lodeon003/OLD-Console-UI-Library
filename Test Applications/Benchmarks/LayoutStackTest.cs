using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lodeon.Terminal.UI.Layout;
using Lodeon.Terminal.Graphics;
using Lodeon.Terminal.UI.Units;
using Lodeon.Terminal;
using Lodeon.Terminal.Graphics.Drivers;

namespace Benchmarks
{
    internal class LayoutStackTest
    {
        public void Main()
        {
            Driver driver = new AnsiDriver();

            LayoutStack.Orientation orientation = LayoutStack.Orientation.Horizontal;
            LayoutStack.HorizontalAlign horizontalAlign = LayoutStack.HorizontalAlign.Left;
            LayoutStack.VerticalAlign verticalAlign = LayoutStack.VerticalAlign.Top;
            int width = 40, height = 10;
            LayoutStack stack;
            
            GraphicBuffer stackBuffer = new GraphicBuffer(width, height);
            GraphicCanvas stackCanvas = new GraphicCanvas(stackBuffer);
            stackCanvas.DrawEmptyRectangle(new(0, 0, stackBuffer.Width, stackBuffer.Height), Color.FromRGB(255, 0, 0));

            List<Element> elements = new List<Element>()
            {
                new Element(new(5, 5), new(0,0,0,0), Color.Random()),
                new Element(new(5, 5), new(0, 0, 0, 0), Color.Random()),
                new Element(new(5, 5), new(0, 0, 0, 0), Color.Random()),
                new Element(new(5, 5), new(0, 0, 0, 0), Color.Random()),
                new Element(new(5, 5), new(0, 0, 0, 0), Color.Random())
            };

            while (true)
            {
                ConsoleKeyInfo key = driver.GetKeyDown();

                switch (key.Key)
                {
                    case ConsoleKey.O:
                        orientation++;
                        if (orientation > LayoutStack.Orientation.Vertical)
                            orientation = LayoutStack.Orientation.Horizontal;
                        break;

                    case ConsoleKey.H:
                        horizontalAlign++;
                        if (horizontalAlign > LayoutStack.HorizontalAlign.Right)
                            horizontalAlign = LayoutStack.HorizontalAlign.Left;
                        break;

                    case ConsoleKey.V:
                        verticalAlign++;
                        if (verticalAlign > LayoutStack.VerticalAlign.Bottom)
                            verticalAlign = LayoutStack.VerticalAlign.Top;
                        break;

                    case ConsoleKey.C:
                        foreach (Element e in elements)
                        {
                            e.buffer.Fill(Color.Random());
                        }
                        break;

                }

                stack = new LayoutStack(new(20, 20), new(10, 10), horizontalAlign, verticalAlign, orientation);
                driver.Clear();
                driver.Display(stackBuffer, stackBuffer.GetSourceArea(), stack.Position);

                foreach (Element e in elements)
                {
                    PixelPoint position = stack.Add(e.size, e.margin);

                    driver.Display(e.buffer.GetGraphics(), e.buffer.GetSourceArea(), position);
                }
            }
        }
    }
}
