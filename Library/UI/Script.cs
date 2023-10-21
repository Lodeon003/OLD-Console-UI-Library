//using Lodeon.Terminal.UI.Layout;
using Lodeon.Terminal.UI.Layout.Presets;
using Lodeon.Terminal.UI.Navigation;
using Lodeon.Terminal.UI.Page;
using Lodeon.Terminal.UI.Paging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Lodeon.Terminal.UI;

/// <summary>
/// The base class for console applications that make use of this' UI library
/// </summary>
public abstract class Script
{
    // Events
    public delegate void EmptyDel();
    public delegate void PageDel(Page page);
    public event EmptyDel? OnExiting;
    internal event PageDel? OnPageChanged;

    // Thread-safe variables
    private Syncronized<bool> _executing = new Syncronized<bool>(false);
    private Syncronized<Page?> _currentPage = new Syncronized<Page?>();
    private CancellationTokenSource? _exitSource;
    
    // "Readonly" variables after initialization
    private ExceptionHandler? _exceptionHandler;
    private Navigator<string, Page>? _navigator;
    private GraphicBuffer? _outputBuffer;
    private Driver? _output;

// --------- PRIVATE METHODS

    private void Initialize(ReadOnlySpan<Exception> initialExceptions)
    {
        //_pages = new Dictionary<string, Page>();
        _exitSource = new CancellationTokenSource();
        _outputBuffer = new GraphicBuffer();
        
        _exceptionHandler = new ExceptionHandler();
        _exceptionHandler.ExceptionThrown += ExceptionHandler_OnThrow;
        _exceptionHandler.ExceptionLogged += ExceptionHandler_OnLog;

        // Pages can add exceptions to the handler.
        Dictionary<string, Page> pages = new Dictionary<string, Page>();
        _navigator = new Navigator<string, Page>(pages);

        PageInitializer pageInit = new PageInitializer(pages, _navigator, _output, _outputBuffer, this, _exceptionHandler);
        this.OnInitialize(pageInit);

        _exceptionHandler.Log(initialExceptions);

        _navigator.OnNavigate += PageNavigator_OnNavigate;
        _navigator.OnNavigateFail += PageNavigator_OnFail;
        _navigator.OnExit += PageNavigator_OnExit;

        _executing.Set(true);

        // If errors occur invoke the OnInitializationFailed method
        if(_exceptionHandler.Exceptions.Count > 0)
        {
            this.OnInitializationFailed(_exceptionHandler.Exceptions);
            return;
        }

        // Code post - execution
        // should run main here.
        _navigator.Navigate((page) => page.IsMain);
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

        try
        {
            await Task.WhenAll(exitTask);
        }
        catch (OperationCanceledException) { }
    }
    private void Exit()
    {
        if (_exitSource == null)
            return;

        try
        {
            _exitSource.Cancel();
        }
        catch (ObjectDisposedException) { }

        // Clear garbage
        _exitSource.Dispose();
        _exitSource = null;

        OnExiting?.Invoke();
        OnExit();
    }

//  --------- PAGE NAVIGATOR and EXCEPTION HANDLER --------------------------------------

    private void PageNavigator_OnFail(Navigator.ErrorCode error)
    {
        ThrowIfNotExecuting();   
        _exceptionHandler.Log(new Exception("No page found with specified parameters"));
    }
    private void PageNavigator_OnNavigate(Page newPage)
    {
        ThrowIfNotExecuting();

        if (_currentPage == newPage)
            return;

        _currentPage.Set(newPage);
        OnPageChanged?.Invoke(newPage);
    }
    private void PageNavigator_OnExit()
        => Exit();

    private void ExceptionHandler_OnThrow(Exception e)
    {
        ThrowIfNotExecuting();
        _currentPage.Get().Popup("Critical Error", e.Message);
        Exit();
    }
    private void ExceptionHandler_OnLog(Exception e)
    {
        ThrowIfNotExecuting();
        _currentPage.Get().Popup("Error", e.Message);
    }


//  --------- OVERRIDABLE METHODS --------------------------------------

    protected virtual void OnInitializationFailed(IReadOnlyCollection<Exception> exceptions)
    {
        ThrowIfNotExecuting();
        _navigator.Navigate(new ErrorPage(exceptions));
    }   

    protected virtual void OnExit() { }
    protected virtual void Main() { }
    protected abstract void OnInitialize(PageInitializer pages);

//  --------- STATIC METHODS --------------------------------------

    public static async Task Run<T, TDriver>() where T : Script, new() where TDriver : Driver, new()
    {
        //ArgumentNullException.ThrowIfNull(customDriver);

        T program = new T();

        Driver driver;
        List<Exception> initialExceptions = new List<Exception>();

        try
        {
            driver = new TDriver();
        }
        catch(Exception e)
        {
            initialExceptions.Add(e);
        }

        program.Initialize(CollectionsMarshal.AsSpan(initialExceptions));
        await program.Wait();
    }
    public static async Task Run<T>() where T : Script, new()
    {
        T program = new T();

        Driver driver;
        List<Exception> initialExceptions = new List<Exception>();

        try
        {
            driver = Driver.GetDefaultDriver();
        }
        catch (Exception e)
        {
            initialExceptions.Add(e);
        }

        program.Initialize(CollectionsMarshal.AsSpan(initialExceptions));
        await program.Wait();
    }
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