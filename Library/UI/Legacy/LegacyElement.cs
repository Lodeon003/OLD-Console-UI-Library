using Lodeon.Terminal.Graphics.Drivers;
using System;
using System.Drawing;

namespace Lodeon.Terminal.UI;

/// <summary>
/// [!] Never tested
/// </summary>
public abstract class LegacyElement : Element
{
    public static GraphicBuffer SharedBuffer { get; } = new GraphicBuffer();

    public LegacyPage Page { get; private set; }
    public Point Position { get; private set; }
    public short Height { get; private set; }
    public short Width { get; private set; }
    public bool IsVisible { get; private set; } = true;
    public bool IsEnabled { get; private set; } = false;
   

    public LegacyElement(LegacyPage page)
    {
        if (page == null)
            throw new ArgumentNullException(nameof(page));

        Page = page;
    }

    public LegacyElement(short x, short y, short width, short height, LegacyPage page)
    {
        if (page == null)
            throw new ArgumentNullException(nameof(page));

        Page = page;

        Position = new Point(x, y);
        Width = width;
        Height = height;
    }

    public void SetPosition(int x, int y)
    {
        OnBeforeChangePosition();
        Position = new Point(x, y);
        OnChangePosition();
    }
    public void SetPosition(Point position)
    {
        OnBeforeChangePosition();
        Position = position;
        OnChangePosition();
    }
    public void SetSize(int width, int height)
    {
        Width = (short)width;
        Height = (short)height;
        OnChangeSize();
    }
    public void SetWidth(int value)
    {
        Width = (short)value;
        OnChangeSize();
    }
    public void SetHeight(int value)
    {
        Height = (short)value;
        OnChangeSize();
    }
    /// <summary>
    /// Method called by a <see cref="Page"/> object when this <see cref="LegacyElement"/> is added to the page via <see cref="LegacyPage.RegisterElement(LegacyElement)"/>
    /// In this method every item should register all ui buffers to the page via <see cref="LegacyPage.RegisterBuffer(LegacyBuffer)"/>
    /// </summary>
    internal abstract void OnRegister();

    /// <summary>
    /// Method called by a <see cref="Page"/> object when this <see cref="LegacyElement"/> is removed from a page via <see cref="LegacyPage.UnregisterElement(LegacyElement)"/><br/>
    /// In this method every item should unregister their UI buffers to the page via <see cref="LegacyPage.UnregisterBuffer(LegacyBuffer)"/>
    /// </summary>
    internal abstract void OnUnregister();
    protected virtual void OnBeforeChangePosition() { }
    protected virtual void OnChangePosition() { }
    protected virtual void OnChangeSize() { }
    protected virtual void OnEnable() { }
    protected virtual void OnDisable() { }

    /// <summary>
    /// Display the buffer to the screen
    /// </summary>
    /// <param name="buffer"></param>
    private protected void Display(LegacyBuffer buffer)
    {
        if (!IsEnabled || !IsVisible)
            return;

        Display(buffer);
    }

    /// <summary>
    /// Clear the buffer form the screen<br/>
    /// (This method calculates and displays buffers behind the one specified without displaying itself)
    /// </summary>
    /// <param name="buffer"></param>
    private protected void Clear(LegacyBuffer buffer)
    {
        //if (!IsEnabled || !IsVisible)   // I think this function should work anyway as it doesn't show the object itself
        //    return;

        throw new NotImplementedException("Legacy elements dropped due to lazyness");
        //Display(buffer.GetGraphics(false), buffer.GetSourceArea(), buffer.Position);
    }

    protected Point LocalToScreen(Point localPoint)
        => new Point(localPoint.X + this.Position.X, localPoint.Y + this.Position.Y);

    private protected void Display(LegacyBuffer buffer, Point offset, Point size)
    {
        if (!IsEnabled || !IsVisible)
            return;

        Point screenPos = LocalToScreen(offset);
        throw new NotImplementedException("Legacy elements dropped due to lazyness");
        //Display(buffer, new Rectangle(screenPos.X, screenPos.Y, size.X, size.Y), size);
    }

    private protected void Display(LegacyBuffer buffer, Rectangle screenArea)
    {
        if (!IsEnabled || !IsVisible)
            return;

        throw new NotImplementedException("Legacy elements dropped due to lazyness");
        //Display(buffer, screenArea);
    }

    public void SetVisible(bool visible)
        => IsVisible = visible;

    public void SetEnabled(bool enable)
    {
        if (IsEnabled == enable)
            return;

        IsEnabled = enable;

        if (IsEnabled)
            OnEnable();
        else
            OnDisable();
    }

    internal override ReadOnlySpan<Element> GetChildren()
        => null;

    public override ReadOnlySpan<Pixel> GetGraphics()
        => null;

    public override Units.PixelPoint GetPosition()
    {
        throw new NotImplementedException();
    }

    public override Rectangle GetScreenArea()
    {
        throw new NotImplementedException();
    }

    public override Units.PixelPoint GetSize()
    {
        throw new NotImplementedException();
    }
}