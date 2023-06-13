using Lodeon.Terminal.UI.Layout;
using Lodeon.Terminal.Graphics;
using Lodeon.Terminal.UI.Units;
using Lodeon.Terminal;
using Lodeon.Terminal.Graphics.Drivers;
using Benchmarks;
using Benchmarks.TreeNavigator;
using Lodeon.Terminal.UI;

Driver driver = new AnsiDriver();
NavContainer parent = new(null, 0, 0, 30, 10, Color.White);
NavElement one = new NavElement(parent, 0, 0, 5, 5, Color.FromRGB(60, 60, 60));
NavElement two = new NavElement(parent, 7, 0, 5, 5, Color.FromRGB(60, 60, 60));
NavElement three = new NavElement(parent, 14, 0, 5, 5, Color.FromRGB(60, 60, 60));
parent.AddChildren(one, two, three);

TreeNavigator Navigator = new(parent);
Navigator.OnSelect += OnSelect;
Navigator.OnDeselect += OnDeselect;
Navigator.OnNavigateFrom += OnNavigateFrom;
Navigator.OnNavigateTo += OnNavigateTo;

parent.Display(driver);

for(int i = 0; i < parent.GetChildren().Length; i++)
    ((Nav)parent.GetChildren()[i]).Display(driver, Color.Black);

while(true)
{
    ConsoleKeyInfo info = Console.ReadKey(true);

    switch(info.Key)
    {
        case ConsoleKey.LeftArrow:
            Navigator.Previous();
            break;

        case ConsoleKey.RightArrow:
            Navigator.Next();
            break;

        case ConsoleKey.UpArrow:
            Navigator.NavigateOut();
            break;

        case ConsoleKey.DownArrow:
            Navigator.NavigateIn();
            break;

        case ConsoleKey.Escape:
            return;
    }
}

void OnNavigateTo(INavigable element)
{
    Nav nav = ((Nav)element);
    nav.Display(driver, Color.FromRGB(255, 0, 0));
}

void OnNavigateFrom(INavigable? element)
{
    Nav nav = ((Nav)element);
    nav.Display(driver, Color.FromRGB(255, 255, 0));
}

void OnDeselect(INavigableElement element)
{
    NavElement nav = (NavElement)element;
    nav.Display(driver, Color.FromRGB(0, 255, 255));
}

void OnSelect(INavigable element)
{
    NavElement nav  = (NavElement)element;
    nav.Display(driver, Color.FromRGB(0, 0, 255));
}
//LayoutStackTest test = new LayoutStackTest();
//test.Main();

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