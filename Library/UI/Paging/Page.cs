using Lodeon.Terminal.Graphics;
using Lodeon.Terminal.UI.Layout;
using Lodeon.Terminal.UI.Navigation;
using Lodeon.Terminal.UI.Units;
using System.Security.Cryptography;
using System.Xml;

namespace Lodeon.Terminal.UI.Paging;

public class Page : Container<Page.InitializationContext>
{
    // ---- Overridable Methods ----------------------------------------------------------------------------

    public virtual void Popup(string title, string text) { }
    protected virtual void OnSelect() { }
    protected virtual void OnLoad() { }
    protected virtual void OnDeselect() { }


    // ------ Variables & Properties ----------------------------------------------------------------------------

    private Script _script;
    private Navigator<string, Page>? _navigator;
    private ExceptionHandler? _exceptionHandler;
    private GraphicBuffer? _programBuffer;

    public bool IsMain { get; init; }
    public event EventHandler<ConsoleKeyInfo> OnKeyDown;

    // ---- Constructors ----------------------------------------------------------------------------

    public Page(InitializationContext context) : base(context)
    {
        
    }


    // ---- Private/Sealed Methods ----------------------------------------------------------------------------
    
    protected sealed override void OnDraw(GraphicCanvas canvas, int width, int height)
    {
        canvas.Fill(Color.FromRGB(255, 255, 204));
        throw new NotImplementedException("Draw all objects");
    }

    private void Display(IElement element)
    {
        // Clear buffer
        // Overlay all element's parents recursively
        // Overlay all element's children recursively
        // Call script function to draw buffer

        // NOTE: It is wrong to give pages the program's buffer, but it is better to only have one instead
        // of many. Make it so only selected page can call buffer display. Maybe fire a 'DisplayRequest' event
        // that the program will listen to
        throw new NotImplementedException();
    }

    // ---- Event Handlers ----------------------------------------------------------------------------
    private EventHandler<ConsoleKeyInfo> Script_KeyDownHandler = (object? sender, ConsoleKeyInfo e) => OnKeyDown?.Invoke(this, e);

    private void Script_OnPageChanged(Page page)
    {
        if(page == this)
        {
            // Enable all elements in page
            // Send a resize event to redraw all screen
            // Hook Script's input events
            // start loop
            _script.OnKeyDown += Script_KeyDownHandler;
            return;
        }

        // Disable all elements in page
        // Unhook all script's input events
        // enable / disable input?

        _script.OnKeyDown -= Script_KeyDownHandler;
        throw new NotImplementedException();
    }
    
    private void Script_OnExit()
    {
        OnDeselect();
        _script.OnExiting -= Script_OnExit;
    }

    // ---- Static Methods ----------------------------------------------------------------------------

    public static Page FromXML(string path, InitializationContext pageContext)
    {
        XmlDocument xml = new XmlDocument();
        xml.Load(path);

        throw new NotImplementedException("Not implemented");

        Page page = new Page(pageContext);
        return page;
    }

    // ---- Initialization Context ----------------------------------------------------------------------------

    public class InitializationContext : Container.InitializationContext
    {

    }

    #region OLD CODE
    /// <summary>
    /// Can't be put in constructor because a generic type deriving from <see cref="Page"/> can't specify a constructor
    /// with parameters
    /// </summary>
    /// <param name="isMain"></param>
    //internal void Initialize(Script program, bool isMain, GraphicBuffer programBuffer, ExceptionHandler handler, Navigator<string, Page> navigator)
    //{
    //    IsMain = isMain;
    //    _programBuffer = programBuffer;
    //    _script = program;
    //    _exceptionHandler = handler;
    //    _navigator = navigator;
    //
    //    _script.OnPageChanged += Script_OnPageChanged;
    //    _script.OnExiting += Script_OnExit;
    //}
    // These two: to implement
    //protected abstract void Main();
    //protected abstract void OnExit();

    //protected virtual void OnInitialize() { }

    //public async Task Execute(CancellationToken token)
    //{
    //    Task mainTask = Task.Run(OnSelect, token);
    //    Task waitTask = token.WaitAsync();
    //
    //    try
    //    {
    //        await Task.WhenAll(mainTask, waitTask);
    //    }
    //    catch (OperationCanceledException) { }
    //}
    #endregion
}