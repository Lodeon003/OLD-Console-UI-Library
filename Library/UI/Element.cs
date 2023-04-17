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
public abstract class Element : ITransform, IRenderable
{
    protected GraphicBuffer Buffer { get { if (_buffer == null) throw new Exception("Internal error. Element was used before initializing using \"Initialize\" function"); return _buffer; } }
    protected GraphicCanvas Canvas { get { if (_canvas == null) throw new Exception("Internal error. Element was used before initializing using \"Initialize\" function"); return _canvas; } }
    protected Page Page { get { if (_page == null) throw new Exception("Internal error. Element was used before initializing using \"Initialize\" function"); return _page; } }
    protected ReadonlyGraphicBuffer CanvasView { get { if (_canvasView == null) throw new Exception("Internal error. Element was used before initializing using \"Initialize\" function"); return _canvasView; } }
    public ITransform Parent { get; private init; }

    private GraphicCanvas? _canvas;
    private GraphicBuffer _buffer;
    private Page? _page;
    private ReadonlyGraphicBuffer? _canvasView;

    public delegate void DisplayRequestedDel(ReadonlyGraphicBuffer graphic);
    public event DisplayRequestedDel? DisplayRequested;

    public void Initialize(Page page, GraphicBuffer buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        _buffer = buffer;
        _canvas = new GraphicCanvas(buffer);

        ArgumentNullException.ThrowIfNull(page);
        _page = page;

        ArgumentNullException.ThrowIfNull(parent);
        _parent = parent;

        _canvasView = _canvas.AsReadonly();
    }

    protected void Display()
    {
        Pixel[] array = ArrayPool<Pixel>.Shared.Rent(buffer.Length);
        GraphicBuffer outBuffer = new GraphicBuffer(buffer.Position, array, Buffer.Length); // Allocate on stack?
        buffer.Fill(Pixel.Empty);

        OverlayParents(outBuffer);
        buffer.Overlay(outBuffer);
        OverlayChildren(outBuffer);

        _driver.Display(outBuffer);
    }

    private void OverlayChildren(GraphicBuffer output)
    {
        ReadOnlySpan<Element> children = GetChildren();

        for(int i = 0; i < children; i++)
            output.Overlay(children[i]);

        for(int i = 0; i < children; i++)
            children[i].OverlayChildren(output);
    }

    private void OverlayParents(GraphicBuffer output)
    {
        // Recursive. Arrive to root of element tree
        if(Parent is Element)
           OverlayParents(output);

        output.Overlay(Buffer);
    }

    public ReadOnlySpan<Pixel> GetGraphics()
        => _buffer.GetGraphics();

    public Point GetScreenArea()
        => _buffer.GetScreenArea();
}
