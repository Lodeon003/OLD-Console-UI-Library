using Lodeon.Terminal.Graphics;
using Lodeon.Terminal.UI.Layout;
using Lodeon.Terminal.UI.Units;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
    public ITransform Parent { get { if (_parent == null) throw new Exception("Internal error. Element was used before initializing using \"Initialize\" function"); return _parent; } }
    public Element[] Children { get { if (_children == null) throw new Exception("Internal error. Element was used before initializing using \"InitChildren\" function"); return _children; } }

    private ReadonlyGraphicBuffer? _canvasView;
    private GraphicCanvas? _canvas;
    private GraphicBuffer? _buffer;
    private Page? _page;
    private ITransform? _parent;
    private Element[]? _children;

    public delegate void DisplayRequestedDel(ReadonlyGraphicBuffer graphic);
    public event DisplayRequestedDel? DisplayRequested;
    public event TransformChangedEvent? PositionChanged;
    public event TransformChangedEvent? SizeChanged;

    public void Initialize(Page page, GraphicBuffer buffer, ITransform parent, Element[] children)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        _buffer = buffer;
        _canvas = new GraphicCanvas(buffer);

        ArgumentNullException.ThrowIfNull(page);
        _page = page;

        ArgumentNullException.ThrowIfNull(parent);
        _parent = parent;

        _children = children;

        _canvasView = _canvas.AsReadonly();
    }

    protected void Display()
        => Page.Display(this);

    public ReadOnlySpan<Pixel> GetGraphics()
        => Buffer.GetGraphics();

    public Rectangle GetScreenArea()
        => Buffer.GetScreenArea();

    public abstract PixelPoint GetPosition();

    public abstract PixelPoint GetSize();

    internal ReadOnlySpan<Element> GetChildren()
        => Children;

    internal static RootElement? TreeFromXml(string path, Page page, GraphicBuffer buffer)
    {
        XmlDocument document = new XmlDocument();
        document.Load(path);

        RootElement root = new RootElement();
        Element[] elements = new Element[document.ChildNodes.Count];

        for (int i = 0; i < document.ChildNodes.Count; i++)
            elements[i] = FromNode(document.ChildNodes.Item(i), page, buffer, root);

        root.Initialize(page, buffer, page, elements);
        return root;
    }

    private static Element FromNode(XmlNode node, Page page, GraphicBuffer buffer, ITransform parent)
    {
        Element[] children = new Element[node.ChildNodes.Count];

        if (node.ChildNodes.Count == 0)
            for (int i = 0; i < node.ChildNodes.Count; i++)
                children[i] = FromNode(node.ChildNodes.Item(i), page, buffer, parent);

        Element element = ElementCache.Instance.Instantiate(node.Name);

        // Get all properties in element type
        foreach (PropertyInfo prop in element.GetType().GetProperties().Where((x) => x.GetSetMethod(true) != null && x.GetGetMethod(true) != null))
        {
            XmlAttribute? attribute = node.Attributes?[prop.Name];

            // If property has been specified in XML
            if (attribute == null)
                continue;

            // [!] Convert value to real type instead of string before converting
            prop.SetValue(element, attribute.Value);
        }

        element.Initialize(page, buffer, parent, children);
        return element;
    }
}
