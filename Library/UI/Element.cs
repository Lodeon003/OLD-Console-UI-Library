using Lodeon.Terminal.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lodeon.Terminal.UI;

/// <summary>
/// Base class for UI elements 
/// </summary>
public abstract class Element
{
    protected GraphicCanvas Canvas { get { if (_canvas == null) throw new Exception("Internal error. Element was used before initializing using \"Initialize\" function"); return _canvas; } }
    protected Page Page { get { if (_page == null) throw new Exception("Internal error. Element was used before initializing using \"Initialize\" function"); return _page; } }
    protected ReadonlyGraphicBuffer CanvasView { get { if (_canvasView == null) throw new Exception("Internal error. Element was used before initializing using \"Initialize\" function"); return _canvasView; } }

    private GraphicCanvas? _canvas;
    private Page? _page;
    private ReadonlyGraphicBuffer? _canvasView;

    public delegate void DisplayRequestedDel(ReadonlyGraphicBuffer graphic);
    public event DisplayRequestedDel? DisplayRequested;

    public void Initialize(GraphicCanvas canvas, Page page)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        _canvas = canvas;

        ArgumentNullException.ThrowIfNull(page);
        _page = page;

        _canvasView = _canvas.AsReadonly();
    }

    protected void Display()
        => DisplayRequested?.Invoke(CanvasView);
}
