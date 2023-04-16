using System.Drawing;

namespace Lodeon.Terminal.UI;


/// <summary>
/// [!] Never tested
/// </summary>
public class LegacyPage : IContainer<LegacyElement>
{
    /// <summary>
    /// [!] Shouldn't allow for semi transparent colors
    /// </summary>
    public Color Background { get; set; } = Color.White;
    public Color Foreground { get; set; } = Color.Black;
    public string Name { get; set; } = string.Empty;

    private List<LegacyElement> _elements = new List<LegacyElement>();
    private List<LegacyBuffer> _buffers = new List<LegacyBuffer>();

    public void AddItem(LegacyElement element)
        => RegisterElement(element);
    private void RegisterElement(LegacyElement element)
    {
        element.OnRegister();
        _elements.Add(element);
    }

    public void RemoveItem(LegacyElement element)
        => UnregisterElement(element);
    private void UnregisterElement(LegacyElement element)
    {
        element.OnUnregister();
        _elements.Remove(element);
    }
    public ReadOnlySpan<LegacyElement> GetItems()
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
        foreach (LegacyElement element in _elements)
            element.SetEnabled(false);
    }

    private void OnSelected()
    {
        foreach (LegacyElement element in _elements)
            element.SetEnabled(true);
    }

    /// <summary>
    /// Should be only called by an <see cref="LegacyElement"/> when registering to a page
    /// </summary>
    internal void RegisterBuffer(LegacyBuffer buffer)
    {
        if (_buffers.Contains(buffer))
            throw new ArgumentException("An UI element tried to register a buffer that was already registered to this page", nameof(buffer));

        _buffers.Add(buffer);
        buffer.OnScreenAreaChanged += OnBufferAreaChanged;
    }

    /// <summary>
    /// Should be only called by an <see cref="LegacyElement"/> when unregistering from a page
    /// </summary>
    internal void UnregisterBuffer(LegacyBuffer buffer)
    {
        if (!_buffers.Contains(buffer))
            throw new ArgumentException("An UI element tried to unregister a buffer that was never registered to this page", nameof(buffer));

        _buffers.Remove(buffer);
    }

    /// <summary>
    /// Handler of event <see cref="LegacyBuffer.OnScreenAreaChanged"/>. Called whenever a buffer in the <see cref="_buffers"/> list changes position, width or height
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="screenArea"></param>
    private void OnBufferAreaChanged(LegacyBuffer sender, Rectangle screenArea)
    {
        foreach (LegacyBuffer buffer in _buffers)
        {
            sender.CheckOverlappingBuffer(buffer, buffer.GetScreenArea());
            buffer.CheckOverlappingBuffer(sender, screenArea);
        }
    }
}


