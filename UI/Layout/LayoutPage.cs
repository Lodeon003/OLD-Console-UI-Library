using Lodeon.Terminal.UI.Units;
using System.Reflection;

namespace Lodeon.Terminal.UI.Layout;

public abstract class LayoutPage : Page, ITransform
{
    private const string UIResourceAttributeName = "Layout";
    private LayoutElement? _root;

    public event TransformChangedEvent? PositionChanged;
    public event TransformChangedEvent? SizeChanged;

    public PixelPoint Position { get; set; }
    public PixelPoint Size { get; set; }

    public PixelPoint GetPosition()
        => Position;

    public PixelPoint GetSize()
        => Size;

    protected sealed override void Load()
    {
        ResourceAttribute? layoutAttribue = GetType().GetCustomAttributes<ResourceAttribute>().Where((a) => a.Name == UIResourceAttributeName).First();

        string path = layoutAttribue is not null ? $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/{layoutAttribue.Path}"
                                                : $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/{GetType().Name}.xml";
        // Import/copy classes form other library
        LayoutElement? root = LayoutElement.TreeFromXml(path);

        if (root is null)
            throw new Exception($"Runtime Error: Couldn't load missing layout file in position: {path}");

        _root = root;
    }
}
