using System.Drawing;

namespace Lodeon.Terminal.UI;

// Add two anchored UI Buffer based on parent size

/// <summary>
/// [!] Never tested
/// </summary>
internal class LegacyBuffer : GraphicBuffer
{
    public static GraphicBuffer SharedBuffer { get; } = new GraphicBuffer();

    public LegacyBuffer(LegacyElement parent, short baseWidth, short baseHeight) : base(baseWidth, baseHeight)
    {
        if (parent == null)
            throw new ArgumentNullException($"Buffers should be contained in {nameof(LegacyElement)}. A buffer shouldn't be created without a parent object");
        Parent = parent;
    }
    public LegacyBuffer(LegacyElement parent, short baseWidth, params Pixel[] buffer) : base(baseWidth, buffer)
    {
        if (parent == null)
            throw new ArgumentNullException($"Buffers should be contained in {nameof(LegacyElement)}. A buffer shouldn't be created without a parent object");
        Parent = parent;
    }
    public LegacyBuffer(LegacyElement parent, short baseWidth, short baseHeight, params Pixel[] buffer) : base(baseWidth, baseHeight, buffer)
    {
        if (parent == null)
            throw new ArgumentNullException($"Buffers should be contained in {nameof(LegacyElement)}. A buffer shouldn't be created without a parent object");
        Parent = parent;
    }
    public LegacyBuffer(LegacyElement parent, Rectangle screenArea, Pixel[] buffer) : base(screenArea, buffer)
    {
        if (parent == null)
            throw new ArgumentNullException($"Buffers should be contained in {nameof(LegacyElement)}. A buffer shouldn't be created without a parent object");
        Parent = parent;
    }
    public LegacyBuffer(LegacyElement parent, Rectangle screenArea) : base(screenArea)
    {
        if (parent == null)
            throw new ArgumentNullException($"Buffers should be contained in {nameof(LegacyElement)}. A buffer shouldn't be created without a parent object");
        Parent = parent;
    }
    public LegacyBuffer(LegacyElement parent)
    {
        if (parent == null)
            throw new ArgumentNullException($"Buffers should be contained in {nameof(LegacyElement)}. A buffer shouldn't be created without a parent object");
        Parent = parent;
    }

    internal delegate void ResizeDelegate(LegacyBuffer sender, short width, short height);
    internal event ResizeDelegate? OnResize;

    internal delegate void MoveDelegate(LegacyBuffer sender, int x, int y);
    internal event MoveDelegate? OnMove;

    /// <summary>
    /// Delegate for <see cref="OnScreenAreaChanged"/> event
    /// </summary>
    /// <param name="sender">The object that invoked the event</param>
    /// <param name="screenArea">The new portion of screen occupied by the object (after size is updated)</param>
    internal delegate void ScreenAreaChangeDelegate(LegacyBuffer sender, Rectangle screenArea);
    internal event ScreenAreaChangeDelegate? OnScreenAreaChanged;

    internal LegacyElement Parent { get; }
    private List<LegacyBuffer> _overlappingBuffers = new List<LegacyBuffer>();
    //private bool _displayForeground = false;

    public override void Resize(short width, short height)
    {
        base.Resize(width, height);
        OnResize?.Invoke(this, width, height);
        OnScreenAreaChanged?.Invoke(this, GetScreenArea());
    }
    public override void SetPosition(Point position)
    {
        base.SetPosition(position);
        OnMove?.Invoke(this, position.X, position.Y);
        OnScreenAreaChanged?.Invoke(this, GetScreenArea());
    }

    /// <summary>
    /// Get the graphics of this buffer. This method overlays all buffers in the same page that are in the same screen area<br/>
    /// [!] Update to use a Z index. Now this buffer will always be on top but some other element could be on top instead.
    /// </summary>
    /// <returns></returns>
    public override ReadOnlySpan<Pixel> GetGraphics()
        => GetGraphics(true);

    /// <summary>
    /// Get the graphics of this buffer. This method overlays all buffers in the same page that are in the same screen area<br/>
    /// If you need to display everything underneath this buffer without this buffer's content set <paramref name="displayItself"/> to false.<br/>
    /// [!] Update to use a Z index. Now this buffer will always be on top but some other element could be on top instead.
    /// </summary>
    /// <returns></returns>
    public ReadOnlySpan<Pixel> GetGraphics(bool displayItself)
    {
        // Resize output buffer if too small
        SharedBuffer.SetPosition(Position);
        SharedBuffer.EnsureSize(Width, Height, false);

        // Fill output buffer with page background
        SharedBuffer.Fill(new Pixel().WithBackground(Parent.Page.Background));

        // Overlay each item under this one to the output buffer
        foreach (var buffer in _overlappingBuffers)
            SharedBuffer.Overlay(buffer);

        // Sometimes just need everything behind the element without the element itself
        if (displayItself)
            SharedBuffer.Overlay(this);

        return SharedBuffer.GetGraphics();
    }

    /// <summary>
    /// If a buffer changes position, the last position must be cleared by drawing everything underneath but not the object itself.<br/>
    /// This prevents <see cref="GetGraphics"/> from drawing the object itself<br/>
    /// [!] Should be reworked to calculate the whole buffer instead of calling <see cref="AnsiDriver.Display(IRenderable, byte)"/> two times
    /// </summary>
    //internal void DrawForeground()
    //{
    //    _displayForeground = true;
    //}

    /// <summary>
    /// Adds a buffer to it's list of overlapping buffers if the area of that other buffer is partially over this buffer's area
    /// <br/>[Called by <see cref="Parent"/>'s <see cref="LegacyPage"/> whenever any buffer in the same page changes position, width or height]
    /// </summary>
    internal void CheckOverlappingBuffer(LegacyBuffer buffer, Rectangle screenArea)
    {
        if (buffer == this)
            return;

        if (buffer.Parent.Page != this.Parent.Page)
            throw new ArgumentException($"Some code tried to check if this buffer was overlapping a buffer from another page.\nThe code calling this method is wrong as they have a reference to a buffer not in it's page");

        if (!screenArea.IntersectsWith(this.GetScreenArea()))
        {
            _overlappingBuffers.Remove(buffer);
            return;
        }

        if (!_overlappingBuffers.Contains(buffer))
            _overlappingBuffers.Add(buffer);
    }
}