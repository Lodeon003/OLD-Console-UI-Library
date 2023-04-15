using Lodeon.Terminal.UI.Layout;

namespace Lodeon.Terminal.UI;

/// <summary>
/// The base class for console applications that make use of this' UI library
/// </summary>
public abstract class Program
{
    public static async Task Run<T>() where T : Program, new()
    {
        T program = new T();
        await program.Execute();
    }

    private SemaphoreSlim? _exitHandle;
    private Dictionary<string, Page>? _pages;

    internal async Task Execute()
    {
        _exitHandle = new SemaphoreSlim(1, 1);
        _pages = new Dictionary<string, Page>();

        OnInitialize(new PageInitializer(_pages));

        bool mainPageFound = false;
        foreach (Page page in _pages.Values)
        {
            if (page.IsMain)
            {
                if (!mainPageFound)
                    mainPageFound = true;
                else
                    throw new Exception("More than one page was selected as the main page");
            }
        }


        Main();
        await _exitHandle.WaitAsync();
    }
    protected void Exit()
    {
        if (_exitHandle == null)
            return;

        try
        {
            _exitHandle.Release();
        }
        catch (ObjectDisposedException) { }

        // Clear garbage
        _exitHandle.Dispose();
        _exitHandle = null;
        _pages = null;
        OnExit();
    }

    protected virtual void OnExit() { }
    protected virtual void Main() { }
    protected abstract void OnInitialize(PageInitializer Pages);
}

/*

class MyProgram : Program
{
    protected override void OnInitialize(PageInitializer Pages)
    {
        Pages.Add<MyMainPage>("MainPage");
    }
}

[Resource("Layout", "Pages/MainPage.xml")]
class MyMainPage : LayoutPage
{
    [Event("Button1", "OnClick")]
    public void Btn1_OnClick()
    {

    }
}
*/