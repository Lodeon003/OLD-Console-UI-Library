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

namespace Benchmarks;

internal class LayoutStackTest
{
    public void Main()
    {
        Driver driver = new AnsiDriver();

        LayoutStackBackup.Orientation orientation = LayoutStackBackup.Orientation.Horizontal;
        LayoutStackBackup.HorizontalAlign horizontalAlign = LayoutStackBackup.HorizontalAlign.Left;
        LayoutStackBackup.VerticalAlign verticalAlign = LayoutStackBackup.VerticalAlign.Top;
        PixelPoint size = new PixelPoint(40, 10);
        PixelPoint position = new PixelPoint(70, 20);
        LayoutStackBackup stack;
        
        GraphicBuffer stackBuffer = new GraphicBuffer(size.X, size.Y);
        GraphicCanvas stackCanvas = new GraphicCanvas(stackBuffer);
        stackCanvas.DrawEmptyRectangle(new(0, 0, stackBuffer.Width, stackBuffer.Height), Color.FromRGB(255, 0, 0));

        List<Element> elements = new List<Element>()
        {
            new Element(new(4, 2), new Pixel4(1, 1, 1, 1), Color.Random()),
            new Element(new(6, 2), new Pixel4(1, 1, 1, 1), Color.Random()),
            new Element(new(4, 4), new Pixel4(1, 1, 1, 1), Color.Random()),
            new Element(new(8, 2), new Pixel4(1, 1, 1, 1), Color.Random()),
            new Element(new(4, 6), new Pixel4(1, 1, 1, 1), Color.Random())
        };

        PixelPoint[] positions = new PixelPoint[elements.Count];

        while (true)
        {
            ConsoleKeyInfo key = driver.GetKeyDown();

            switch (key.Key)
            {
                case ConsoleKey.O:
                    orientation++;
                    if (orientation > LayoutStackBackup.Orientation.Vertical)
                        orientation = LayoutStackBackup.Orientation.Horizontal;
                    break;

                case ConsoleKey.H:
                    horizontalAlign++;
                    if (horizontalAlign > LayoutStackBackup.HorizontalAlign.Right)
                        horizontalAlign = LayoutStackBackup.HorizontalAlign.Left;
                    break;

                case ConsoleKey.V:
                    verticalAlign++;
                    if (verticalAlign > LayoutStackBackup.VerticalAlign.Bottom)
                        verticalAlign = LayoutStackBackup.VerticalAlign.Top;
                    break;

                case ConsoleKey.C:
                    foreach (Element e in elements)
                        e.buffer.Fill(Color.Random());
                    break;

            }
            stack = new LayoutStackBackup(position, size, horizontalAlign, verticalAlign, orientation);
            stack.Calculate(elements.Select(x => x.Layout).ToArray().AsSpan(), positions.AsSpan());

            driver.Clear();
            driver.Display(stackBuffer, stackBuffer.GetSourceArea(), stack.Position);

            for(int i = 0; i < elements.Count; i++)
                driver.Display(elements[i].buffer.GetGraphics(), elements[i].buffer.GetSourceArea(), positions[i]);
        }
    }

}
struct Element
{
    public Element(PixelPoint siz, Pixel4 marg, Color col)
    {
        Layout = new PixelLayout(siz, marg);
        buffer = new GraphicBuffer(siz.X, siz.Y);
        buffer.Fill(col);
    }
    public PixelLayout Layout;
    public GraphicBuffer buffer;
}
