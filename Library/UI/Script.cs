//using Lodeon.Terminal.UI.Layout;
using Lodeon.Terminal.UI.Navigation;
using Lodeon.Terminal.UI.Paging;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Lodeon.Terminal.UI;

/// <summary>
/// Describes how scripts interface with consumer code.<br/><br/>
/// <b>Recommended: </b>  use <see cref="Script"/> or <see cref="Script{TContext}"/> as they offer functionality out of the box.<br/>
/// Derive to entirerly rewrite <see cref="Script"/>'s default UI Application Functionality.
/// </summary>
public interface IScript
{
    protected void Initialize(ReadOnlySpan<Exception> initializationErrors);
    protected abstract Task Wait();

    /// <summary>
    /// [!] UNSAFE: No type checking on script
    /// Runs a new instance of script of type <typeparamref name="TScript"/>
    /// </summary>
    /// <typeparam name="TScript">The type of script to run</typeparam>
    /// <param name="context"></param>
    /// <param name="driverType"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    private static async Task Run(Type scriptType, Type driverType, InitializationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(driverType);

        List<Exception> initialExceptions = new List<Exception>();
        IScript? program;
        Driver? driver;


        // Try to create an instance of the script. Throw if failed
        try {
            program = Activator.CreateInstance(scriptType, context) as IScript;

            if (program == null)
                throw new InvalidOperationException($"The function itself worked, worked but instance is null. It was casted wrong");
        }
        catch (Exception e) {
            throw new InvalidOperationException($"\'{nameof(Activator.CreateInstance)}\' unexpectedly failed:\n -> {e.Message}", e);
        }

        try {
            driver = Activator.CreateInstance(driverType, context) as Driver;
        }
        catch (Exception e) {
            initialExceptions.Add(e);
        }

        // Run program
        program.Initialize(CollectionsMarshal.AsSpan(initialExceptions));
        await program.Wait();
    }

    /// <summary>
    /// Instantiates and runs a script of type <typeparamref name="TScript"/>
    /// </summary>
    /// <typeparam name="TScript">The class of the script to run</typeparam>
    /// <typeparam name="TContext">The class of the parameters to pass to the script</typeparam>
    /// <param name="context">The parameters to pass to the script</param>
    /// <returns>A task will complete when the script stops running</returns>
    public static async Task Run<TScript, TContext>(TContext context) where TScript : Script<TContext>, new() where TContext : InitializationContext, new()
        => await Run(typeof(TScript), Driver.GetDefaultDriverType(), context);

    /// <summary>
    /// Instantiates and runs a script of type <typeparamref name="TScript"/>
    /// </summary>
    /// <typeparam name="TScript">The class of the script to run</typeparam>
    /// <typeparam name="TDriver">The graphic driver used to display graphics</typeparam>
    /// <typeparam name="TContext">The class of the parameters to pass to the script</typeparam>
    /// <param name="context">The parameters to pass to the script</param>
    /// <returns>A task will complete when the script stops running</returns>
    public static async Task Run<TScript, TDriver, TContext>(TContext context) where TScript : Script<TContext>, new() where TDriver : Driver, new() where TContext : InitializationContext, new()
        => await Run(typeof(TScript), typeof(TDriver), context);


    public class InitializationContext { }
}

/// <summary>
/// Derive this class and override functions to create UI Applications.<br/><br/>
/// <typeparamref name="TContext"/> derives from <see cref="IScript.InitializationContext"/> where to put program's start parameters<br/>
/// If no parameters are needed derive the <see cref="Script"/> class
/// </summary>
public abstract class Script<TContext>
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
    private Driver? _driver;

// --------- PRIVATE METHODS

    protected Script(TContext context) { }

    private protected void Initialize(ReadOnlySpan<Exception> initialExceptions)
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

        PageInitializer pageInit = new PageInitializer(pages, _navigator, _outputBuffer, this, _exceptionHandler);
        this.OnInitialize(pageInit);

        _exceptionHandler.Log(initialExceptions);

        _navigator.OnNavigate += PageNavigator_OnNavigate;
        _navigator.OnNavigateFail += PageNavigator_OnFail;
        _navigator.OnExit += PageNavigator_OnExit;

        // [!] Non dovrebbe essere dopo?
        _executing.Set(true);

        // If errors occur invoke the OnInitializationFailed method
        if(_exceptionHandler.Exceptions.Count > 0)
        {
            this.OnInitializationFailed(_exceptionHandler.Exceptions);
            return;
        }

        if(pages.Count <= 0)
        {
            this.OnInitializationFailed(new ReadOnlyCollection<Exception>(new Exception[] { new("No pages where added to this script.\n Try assigning them in the script's overridable method") }));
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

    private void Draw(IElement? element)
    {
        if (element is null)
            return;

        _driver.Display(element);
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

        _currentPage.Lock((page) =>
        {
            if (page != null)
                page.DisplayRequested -= Page_DrawRequested;
        });

        newPage.DisplayRequested += Page_DrawRequested;
        
        _currentPage.Set(newPage);
        OnPageChanged?.Invoke(newPage);
        throw new NotImplementedException("Enable elements.");
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
        Page? page = _currentPage.Get();

        if(page != null)
            page.Popup("Error", e.Message);
    }

    private void Page_DrawRequested(object? sender, EventArgs e)
        => Draw(sender as IElement);


    //  --------- OVERRIDABLE METHODS --------------------------------------

    protected virtual void OnInitializationFailed(IReadOnlyCollection<Exception> exceptions)
    {
        ThrowIfNotExecuting();
        _navigator.Navigate(new ErrorPage(exceptions));
    }   

    protected virtual void OnExit() { }
    protected virtual void Main() { }
    protected abstract void OnInitialize(PageInitializer pages);
}

/// <summary>
/// The base class for console applications that make use of this' UI library<br/>
/// Derive this class and override functions to create UI Applications.<br/><br/>
/// To pass parameters when the script starts use <see cref="Script{TContext}"/> class
/// </summary>
public abstract class Script : Script<IScript.InitializationContext>
{
    protected Script(IScript.InitializationContext context) : base(context) { }
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