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
using Microsoft.Extensions.Logging;

namespace Benchmarks;

internal class LayoutStackTest
{
    Driver driver = new AnsiDriver();
    string msg = "";

    public void Main()
    {
        Debugger.OnLog += OnLog;
        LayoutStackBackup.Orientation orientation = LayoutStackBackup.Orientation.Horizontal;
        LayoutStackBackup.HorizontalAlign horizontalAlign = LayoutStackBackup.HorizontalAlign.Left;
        LayoutStackBackup.VerticalAlign verticalAlign = LayoutStackBackup.VerticalAlign.Top;
        PixelPoint size = new PixelPoint(40, 20);
        PixelPoint position = new PixelPoint(40, 10);
        LayoutStackBackup stack = new LayoutStackBackup(position, size, horizontalAlign, verticalAlign, orientation);

        GraphicBuffer stackBuffer = new GraphicBuffer(size.X, size.Y);
        GraphicCanvas stackCanvas = new GraphicCanvas(stackBuffer);
        stackCanvas.DrawEmptyRectangle(new(0, 0, stackBuffer.Width, stackBuffer.Height), Color.FromRGB(255, 0, 0));

        List<Element> elements = new List<Element>()
        {
            new Element(new(6, 4), new Pixel4(1, 1, 1, 1), Color.Random()),
            new Element(new(4, 6), new Pixel4(1, 1, 1, 1), Color.Random()),
            new Element(new(6, 4), new Pixel4(1, 1, 1, 1), Color.Random()),
            new Element(new(4, 6), new Pixel4(1, 1, 1, 1), Color.Random()),
            new Element(new(6, 4), new Pixel4(1, 1, 1, 1), Color.Random())
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


                case ConsoleKey.Add:
                    elements.Add(new Element(new(Random.Shared.Next(1, 7), Random.Shared.Next(1, 7)), Pixel4.One, Color.Random()));
                    break;

                case ConsoleKey.Subtract:
                    elements.RemoveAt(elements.Count - 1);
                    break;

                case ConsoleKey.LeftArrow:
                    stack.SetPosition(stack.Position.X-3, stack.Position.Y);
                    break;

                case ConsoleKey.RightArrow:
                    stack.SetPosition(stack.Position.X - 3, stack.Position.Y);
                    break;

                case ConsoleKey.UpArrow:
                    stack.SetPosition(stack.Position.X, stack.Position.Y - 3);
                    break;

                case ConsoleKey.DownArrow:
                    stack.SetPosition(stack.Position.X, stack.Position.Y + 3);
                    break;

                case ConsoleKey.A:
                    stack.SetSize(stack.Size.X - 3, stack.Size.Y);
                    break;

                case ConsoleKey.D:
                    stack.SetSize(stack.Position.X - 3, stack.Size.Y);
                    break;

                case ConsoleKey.W:
                    stack.SetSize(stack.Size.X, stack.Size.Y - 3);
                    break;

                case ConsoleKey.S:
                    stack.SetSize(stack.Size.X, stack.Size.Y + 3);
                    break;
            }

            stack = new LayoutStackBackup(position, size, horizontalAlign, verticalAlign, orientation);
            stack.Calculate(elements.Select(x => x.Layout).ToArray().AsSpan(), positions.AsSpan());

            driver.Clear();
            driver.Display(msg, PixelPoint.Zero);
            driver.Display(stackBuffer, stackBuffer.GetSourceArea(), stack.Position);

            for(int i = 0; i < elements.Count; i++)
                driver.Display(elements[i].buffer.GetGraphics(), elements[i].buffer.GetSourceArea(), positions[i]);
        }
    }

    private void OnLog(object? sender, string e)
    {
        msg = e;
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
