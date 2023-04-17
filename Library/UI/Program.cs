using Lodeon.Terminal.UI.Layout;
namespace Lodeon.Terminal.UI;

/// <summary>
/// The base class for console applications that make use of this' UI library
/// </summary>
public abstract class Script
{
    public static async Task Run<T>(Driver customDriver) where T : Script, new()
    {
        T program = new T();
        program.Initialize(customDriver);
        await program.Execute();
    }
    public static async Task Run<T>() where T : Script, new()
    {
        T program = new T();
        program.Initialize(Driver.GetDefaultDriver());
        await program.Execute();
    }


    private Driver? _output;
    private Dictionary<string, Page>? _pages;
    private CancellationTokenSource? _exitSource;
    private Page? _currentPage;
    private GraphicBuffer? _outputBuffer;

    private void Initialize(Driver driver)
    {
        _output = driver;
    }

    private async Task Execute()
    {
        if (_output is null)
            throw new Exception("Internal error. Program was run without it being initialized");

        _pages = new Dictionary<string, Page>();
        _exitSource = new CancellationTokenSource();
        _outputBuffer = new GraphicBuffer();

        PageInitializer pages = new PageInitializer(_pages, _output, _outputBuffer);
        this.OnInitialize(pages);

        Task mainTask = Task.Run(Main, _exitSource.Token);
        Task exitTask = Task.Run(() => WaitHandle.WaitAny(new[] { _exitSource.Token.WaitHandle }));

        try {
            await Task.WhenAll(mainTask, exitTask);
        }
        catch (OperationCanceledException) { }
    }

    protected void Exit()
    {
        if (_exitSource == null)
            return;

        try {
            _exitSource.Cancel();
        }
        catch (ObjectDisposedException) { }

        // Clear garbage
        _exitSource.Dispose();
        _exitSource = null;

        _currentPage.Exit();
        _currentPage = null;
        _pages = null;

        OnExit();
    }

    protected virtual void OnExit() { }
    protected virtual void Main() { }
    protected abstract void OnInitialize(PageInitializer pages);
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