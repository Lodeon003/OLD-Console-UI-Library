using Lodeon.Terminal.UI.Units;
using System.Security.Cryptography;

namespace Lodeon.Terminal.UI;

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

    private Script Script { get { if (_script is null) throw new ArgumentNullException(nameof(Out), "Element was not initialized"); return _script; } }
    private Script? _script;

    protected Navigator<string, Page> Navigator { get { if (_navigator is null) throw new ArgumentNullException(nameof(Navigator), "Element was not initialized"); return _navigator; } }
    private Navigator<string, Page>? _navigator;

    protected ExceptionHandler ExceptionHandler { get { if (_exceptionHandler is null) throw new ArgumentNullException(nameof(ExceptionHandler), "Element was not initialized"); return _exceptionHandler; } }
    private ExceptionHandler? _exceptionHandler;

    private Driver Out { get { if (_driver is null) throw new ArgumentNullException(nameof(Out), "Element was not initialized"); return _driver; } }
    private Driver? _driver;

    protected GraphicBuffer ProgramBuffer { get { if (_programBuffer is null) throw new ArgumentNullException(nameof(ProgramBuffer), "Element was not initialized"); return _programBuffer; } }
    private GraphicBuffer? _programBuffer;

    protected RootElement Root { get { if (_root is null) throw new ArgumentNullException(nameof(RootElement), "Element was not initialized"); return _root; } }
    private RootElement? _root;
    public event TransformChangedEvent PositionChanged;
    public event TransformChangedEvent SizeChanged;

    public bool IsMain { get; private set; }

    /// <summary>
    /// Can't be put in constructor because a generic type deriving from <see cref="Page"/> can't specify a constructor
    /// with parameters
    /// </summary>
    /// <param name="isMain"></param>
    internal void Initialize(Script program, Driver driver, bool isMain, GraphicBuffer programBuffer, ExceptionHandler handler, Navigator<string, Page> navigator)
    {
        _driver = driver;
        IsMain = isMain;
        _programBuffer = programBuffer;
        _script = program;
        _exceptionHandler = handler;
        _navigator = navigator;

        _script.OnExiting += Script_OnExit;
    }

    internal void Display(Element element)
    {
        OverlayParent(element);
        ProgramBuffer.Overlay(element.GetGraphics(), element.GetScreenArea());
        OverlayChildren(element);
        Out.Display(ProgramBuffer);
    }

    private void OverlayChildren(Element element)
    {
        ReadOnlySpan<Element> children = element.GetChildren();

        for(int i = 0; i < children.Length; i++)
        {
            ProgramBuffer.Overlay(children[i].GetGraphics(), children[i].GetScreenArea());
            OverlayChildren(children[i]);
        }
    }

    private void OverlayParent(Element element)
    {
        if (element.Parent != null)
            OverlayParent(element);

        ProgramBuffer.Overlay(element.GetGraphics(), element.GetScreenArea());
    }

    private void Script_OnExit()
    {
        OnDeselect();
        Script.OnExiting -= Script_OnExit;
    }

    internal async Task Execute(CancellationToken token)
    {
        Task mainTask = Task.Run(OnSelect, token);
        Task waitTask = token.WaitAsync();

        try {
            await Task.WhenAll(mainTask, waitTask);
        }
        catch(OperationCanceledException) { }
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