using Lodeon.Terminal.UI.Layout;
using Lodeon.Terminal.UI.Navigation;
using Lodeon.Terminal.UI.Units;
using System.Security.Cryptography;

namespace Lodeon.Terminal.UI.Paging;

public abstract class Page : ITransform
{

    public PixelPoint GetPosition()
    {
        throw new NotImplementedException();
    }

    public PixelPoint GetSize()
    {
        throw new NotImplementedException();
    }

    private Script Script { get { if (_script is null) throw new ArgumentNullException(nameof(Script), "Element was not initialized"); return _script; } }
    private Script? _script;

    protected Navigator<string, Page> Navigator { get { if (_navigator is null) throw new ArgumentNullException(nameof(Navigator), "Element was not initialized"); return _navigator; } }
    private Navigator<string, Page>? _navigator;

    protected ExceptionHandler ExceptionHandler { get { if (_exceptionHandler is null) throw new ArgumentNullException(nameof(ExceptionHandler), "Element was not initialized"); return _exceptionHandler; } }
    private ExceptionHandler? _exceptionHandler;

    protected GraphicBuffer ProgramBuffer { get { if (_programBuffer is null) throw new ArgumentNullException(nameof(ProgramBuffer), "Element was not initialized"); return _programBuffer; } }
    private GraphicBuffer? _programBuffer;

    public event ITransform.PositionChangeDel? PositionChanged;
    public event ITransform.SizeChangeDel? SizeChanged;
    public event ITransform.TransformChangeDel? TransformChanged;

    //protected RootElement Root { get { if (_root is null) throw new ArgumentNullException(nameof(RootElement), "Element was not initialized"); return _root; } }
    //private RootElement? _root;

    public bool IsMain { get; private set; }

    /// <summary>
    /// Can't be put in constructor because a generic type deriving from <see cref="Page"/> can't specify a constructor
    /// with parameters
    /// </summary>
    /// <param name="isMain"></param>
    internal void Initialize(Script program, bool isMain, GraphicBuffer programBuffer, ExceptionHandler handler, Navigator<string, Page> navigator)
    {
        IsMain = isMain;
        _programBuffer = programBuffer;
        _script = program;
        _exceptionHandler = handler;
        _navigator = navigator;

        _script.OnPageChanged += Script_OnPageChanged;
        _script.OnExiting += Script_OnExit;
    }

    private void Script_OnPageChanged(Page page)
    {
        // if page == this
            // Enable all elements in page
            // Send a resize event to redraw all screen
            // Hook Script's input events
            // start loop
        // else
            // Disable all elements in page
            // Unhook all script's input events

        // enable / disable input?
        throw new NotImplementedException();
    }

    internal void Display(IElement element)
    {
        // Clear buffer
        // Overlay all element's parents recursively
        // Overlay all element's children recursively
        // Call script function to draw buffer
        
        // NOTE: It is wrong to give pages the program's buffer, but it is better to only have one instead
        // of many. Make it so only selected page can call buffer display. Maybe fire a 'DisplayRequest' event
        // that the program will listen to

        //ProgramBuffer.Overlay(element.GetGraphics(), element.GetScreenArea());
        //OverlayChildren(element);
        //Out.Display(ProgramBuffer.GetGraphics(), ProgramBuffer.GetArea(), new(0,0));
        throw new NotImplementedException();
    }

    //private void OverlayChildren(IElement element)
    //{
    //    ReadOnlySpan<IElement> children = element.GetChildren();
    //
    //    for (int i = 0; i < children.Length; i++)
    //    {
    //        ProgramBuffer.Overlay(children[i].GetGraphics(), children[i].GetScreenArea());
    //        OverlayChildren(children[i]);
    //    }
    //}

    //private void OverlayParent(IElement element)
    //{
    //    if (element.Parent != null)
    //        OverlayParent(element);
    //
    //    ProgramBuffer.Overlay(element.GetGraphics(), element.GetScreenArea());
    //}

    private void Script_OnExit()
    {
        OnDeselect();
        Script.OnExiting -= Script_OnExit;
    }

    internal async Task Execute(CancellationToken token)
    {
        Task mainTask = Task.Run(OnSelect, token);
        Task waitTask = token.WaitAsync();

        try
        {
            await Task.WhenAll(mainTask, waitTask);
        }
        catch (OperationCanceledException) { }
    }

    /// <summary>
    /// Override to add code before calling events. Call base.OnLoad after your code
    /// </summary>
    protected virtual void Load()
        => OnLoad();

    public abstract void Popup(string title, string text);
    protected abstract void OnSelect();
    protected virtual void OnLoad() { }
    protected abstract void OnDeselect();


    // These two: to implement
    //protected abstract void Main();
    //protected abstract void OnExit();

    //protected virtual void OnInitialize() { }
}