using System.Security.Cryptography;

namespace Lodeon.Terminal.UI;

public abstract class Page
{
    protected Script Program { get { if (_program is null) throw new ArgumentNullException(nameof(Out), "Element was not initialized"); return _program; } }
    private Script? _program;

    protected Driver Out { get { if (_driver is null) throw new ArgumentNullException(nameof(Out), "Element was not initialized"); return _driver; } }
    private Driver? _driver;

    protected GraphicBuffer ProgramBuffer { get { if (_programBuffer is null) throw new ArgumentNullException(nameof(ProgramBuffer), "Element was not initialized"); return _programBuffer; } }
    private GraphicBuffer? _programBuffer;

    public bool IsMain { get; private set; }

    /// <summary>
    /// Can't be put in constructor because a generic type deriving from <see cref="Page"/> can't specify a constructor
    /// with parameters
    /// </summary>
    /// <param name="isMain"></param>
    internal void Initialize(Script program, Driver driver, bool isMain, GraphicBuffer programBuffer)
    {
        _driver = driver;
        IsMain = isMain;
        _programBuffer = programBuffer;
        _program = program;

        _program.OnExiting += ProgramExitCallback;
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

    private void ProgramExitCallback()
    {
        OnDeselect();
        Program.OnExiting -= ProgramExitCallback;
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

    protected abstract void OnSelect();
    protected abstract void Load();
    protected abstract void OnDeselect();
    // These two: to implement
    //protected abstract void Main();
    //protected abstract void OnExit();

    //protected virtual void OnInitialize() { }
}