using Lodeon.Terminal.UI.Layout;
using Lodeon.Terminal.UI.Layout.Presets;
using System.Runtime.CompilerServices;

namespace Lodeon.Terminal.UI;

/// <summary>
/// The base class for console applications that make use of this' UI library
/// </summary>
public abstract class Script
{
    public delegate void EmptyDel();
    public event EmptyDel? OnExiting;

    private CancellationTokenSource? _exitSource;
    private ExceptionHandler? _exceptionHandler;
    private Dictionary<string, Page>? _pages;
    private GraphicBuffer? _outputBuffer;
    private Page? _currentPage;
    private Page? _mainPage;
    private Driver? _output;
    private Syncronized<bool> _executing = new Syncronized<bool>(false);

    private void Execute(Driver driver)
    {
        ArgumentNullException.ThrowIfNull(driver);
        _output = driver;

        _pages = new Dictionary<string, Page>();
        _exitSource = new CancellationTokenSource();
        _outputBuffer = new GraphicBuffer();
        _exceptionHandler = new ExceptionHandler();

        // Pages can add exceptions to the handler.
        PageInitializer pages = new PageInitializer(_pages, _output, _outputBuffer, this, _exceptionHandler);
        this.OnInitialize(pages);

        _executing.Set(true);

        // If errors occur invoke the OnInitializationFailed method
        if(_exceptionHandler.Exceptions.Count > 0)
        {
            this.OnInitializationFailed(_exceptionHandler.Exceptions);
            return;
        }

        // Code post - execution
        // should run main here.
        RunPage(_pages.Where((pair) => pair.Value.IsMain).First().Key);
    }

    public string GetPageName()
    {
        ThrowIfNotExecuting();

        for(int i = int.MinValue; i < int.MaxValue; i++)
        {
            if (_pages.ContainsKey($"Page {i}"))
                continue;

            return $"Page {i}";
        }

        throw new Exception("Internal error. This program contains pages with all number names. Not teoretically possible");
    }

    private void HandleExceptionThrow(Exception e)
    {
        ThrowIfNotExecuting();
        _currentPage.Popup("Critical Error", e.Message);
    }
    private void HandleExceptionLog(Exception e)
    {
        ThrowIfNotExecuting();
        _currentPage.Popup("Error", e.Message);
    }

    private void ThrowIfNotExecuting()
    {
        _executing.Lock((value) =>
        {
            if (!value)
                throw new InvalidOperationException("Internal error, method was called before program was initialized");
        });
    }


    private async Task Wait()
    {
        ThrowIfNotExecuting();

        //Task mainTask = Task.Run(Main, _exitSource.Token);
        Task exitTask = _exitSource.Token.WaitAsync();

        try {
            await Task.WhenAll(exitTask);
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

        _currentPage = null;
        _pages = null;

        OnExiting?.Invoke();
        OnExit();
    }

    public void RunPage(string name)
    {
        ThrowIfNotExecuting();

        if (!_pages.TryGetValue(name, out Page page))
            throw new Exception("No page present with that name");

        // [!] tell to last page to stop
        // execute new page and pass cancellation token
        // set page as current
    }

    public void AddPage(string name, Page page)
    {
        if(_pages.ContainsKey(name))
        {
            _exceptionHandler.Log(new ArgumentException($"A page was already added to the program with name \'{name}\'"));
            return;
        }
    }

    protected virtual void OnInitializationFailed(IReadOnlyCollection<Exception> exceptions)
    {
        string name = GetPageName();
        AddPage(name, new ErrorPage(exceptions));
        RunPage(name);
    }
   
    protected virtual void OnExit() { }
    protected virtual void Main() { }
    protected abstract void OnInitialize(PageInitializer pages);

    public static async Task Run<T>(Driver customDriver) where T : Script, new()
    {
        ArgumentNullException.ThrowIfNull(customDriver);

        T program = new T();
        program.Execute(customDriver);
        await program.Wait();
    }
    public static async Task Run<T>() where T : Script, new()
    => await Run<T>(Driver.GetDefaultDriver());
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