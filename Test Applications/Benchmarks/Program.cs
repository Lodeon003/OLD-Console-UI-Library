using Lodeon.Terminal.UI.Layout;
using Lodeon.Terminal.Graphics;
using Lodeon.Terminal.UI.Units;
using Lodeon.Terminal;

/*Driver driver = Driver.GetDefaultDriver();
LayoutStack stack = new LayoutStack(new(0, 0), new(10,10), LayoutStack.HorizontalAlign.Left, LayoutStack.VerticalAlign.Top);

List<Element> elements = new List<Element>()
{
    new Element(new(2, 2), new(0,0,0,0), Color.FromRGB(255, 0, 0)),
    new Element(new(2, 2), new(0, 0, 0, 0), Color.FromRGB(255, 0, 0))
};

foreach(Element e in elements)
{
    PixelPoint position = stack.Add(e.size, e.margin);

    driver.Display(e.buffer, );
}*/


object locc = new object();
Lock1();
return;

void Lock1()
{
    lock (locc)
    {
        Console.WriteLine("Start lock 1");
        Lock2();
        Console.WriteLine("End lock 1");
    }
}

void Lock2()
{
    lock (locc)
    {
        Console.WriteLine("Test lock 2");
    }
}

struct Element
{
    public Element(PixelPoint siz, Pixel4 marg, Color col)
    {
        size = siz;
        margin = marg;
        color = col;
        position = default;
        buffer = new GraphicBuffer((short)size.X, (short)size.Y);
    }

    public GraphicBuffer buffer;
    public PixelPoint position;
    public PixelPoint size;
    public Color color;
    public Pixel4 margin;
}