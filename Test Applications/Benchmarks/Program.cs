using Lodeon.Terminal.UI.Layout;
using Lodeon.Terminal.Graphics;
using Lodeon.Terminal.UI.Units;
using Lodeon.Terminal;
using Lodeon.Terminal.Graphics.Drivers;
using Benchmarks;

LayoutStackTest test = new LayoutStackTest();
test.Main();

/*object locc = new object();
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
}*/