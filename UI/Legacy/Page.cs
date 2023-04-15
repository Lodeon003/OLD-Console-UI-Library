using System.Drawing;

namespace Lodeon.Terminal.UI;


/// <summary>
/// [!] Never tested
/// </summary>
public class LegacyPage : IContainer<UIElement>
{
    /// <summary>
    /// [!] Shouldn't allow for semi transparent colors
    /// </summary>
    public Color Background { get; set; } = Color.White;
    public Color Foreground { get; set; } = Color.Black;
    public string Name { get; set; } = string.Empty;

    private List<UIElement> _elements = new List<UIElement>();
    private List<UIBuffer> _buffers = new List<UIBuffer>();

    public void AddItem(UIElement element)
        => RegisterElement(element);
    private void RegisterElement(UIElement element)
    {
        element.OnRegister();
        _elements.Add(element);
    }

    public void RemoveItem(UIElement element)
        => UnregisterElement(element);
    private void UnregisterElement(UIElement element)
    {
        element.OnUnregister();
        _elements.Remove(element);
    }
    public ReadOnlySpan<UIElement> GetItems()
        => _elements.ToArray();

    public LegacyPage()
    {
        Output.OnPageChanged += Handler_OnPageChanged;
    }
    ~LegacyPage()
    {
        Output.OnPageChanged -= Handler_OnPageChanged;
    }

    private void Handler_OnPageChanged(LegacyPage newPage)
    {
        if (newPage == this)
            OnSelected();
        else
            OnDeselected();
    }

    private void OnDeselected()
    {
        foreach (UIElement element in _elements)
            element.SetEnabled(false);
    }

    private void OnSelected()
    {
        foreach (UIElement element in _elements)
            element.SetEnabled(true);
    }

    /// <summary>
    /// Should be only called by an <see cref="UIElement"/> when registering to a page
    /// </summary>
    internal void RegisterBuffer(UIBuffer buffer)
    {
        if (_buffers.Contains(buffer))
            throw new ArgumentException("An UI element tried to register a buffer that was already registered to this page", nameof(buffer));

        _buffers.Add(buffer);
        buffer.OnScreenAreaChanged += OnBufferAreaChanged;
    }

    /// <summary>
    /// Should be only called by an <see cref="UIElement"/> when unregistering from a page
    /// </summary>
    internal void UnregisterBuffer(UIBuffer buffer)
    {
        if (!_buffers.Contains(buffer))
            throw new ArgumentException("An UI element tried to unregister a buffer that was never registered to this page", nameof(buffer));

        _buffers.Remove(buffer);
    }

    /// <summary>
    /// Handler of event <see cref="UIBuffer.OnScreenAreaChanged"/>. Called whenever a buffer in the <see cref="_buffers"/> list changes position, width or height
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="screenArea"></param>
    private void OnBufferAreaChanged(UIBuffer sender, Rectangle screenArea)
    {
        foreach (UIBuffer buffer in _buffers)
        {
            sender.CheckOverlappingBuffer(buffer, buffer.GetScreenArea());
            buffer.CheckOverlappingBuffer(sender, screenArea);
        }
    }
}


