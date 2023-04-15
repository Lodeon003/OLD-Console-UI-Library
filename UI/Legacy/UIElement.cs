using System.Drawing;

namespace Lodeon.Terminal.UI;

public enum HorizontalAlignment
{
    Left, Right, Stretch
}

public enum VerticalAlignment
{
    Top, Bottom, Stretch
}

/// <summary>
/// [!] Never tested
/// </summary>
public abstract class UIElement
{
    public static GraphicBuffer SharedBuffer { get; } = new GraphicBuffer();

    public short MinWidth;
    public short MaxWidth;
    public short MinHeight;
    public short MaxHeight;
    public VerticalAlignment VerticalAlignment;
    public HorizontalAlignment HorizontalAlignment;
    // If alignement is 'stretch' min and max values are taken into acount
    // If alignement is fixed size will always be the same size and normal values can be taken into acount

    public LegacyPage Page { get; private set; }
    public Point Position { get; private set; }
    public short Height { get; private set; }
    public short Width { get; private set; }
    public bool IsVisible { get; private set; } = true;
    public bool IsEnabled { get; private set; } = false;

    public UIElement(LegacyPage page)
    {
        if (page == null)
            throw new ArgumentNullException(nameof(page));

        Page = page;
    }
    public UIElement(short x, short y, short width, short height, LegacyPage page)
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
    /// Method called by a <see cref="Page"/> object when this <see cref="UIElement"/> is added to the page via <see cref="LegacyPage.RegisterElement(UIElement)"/>
    /// In this method every item should register all ui buffers to the page via <see cref="LegacyPage.RegisterBuffer(UIBuffer)"/>
    /// </summary>
    internal abstract void OnRegister();

    /// <summary>
    /// Method called by a <see cref="Page"/> object when this <see cref="UIElement"/> is removed from a page via <see cref="LegacyPage.UnregisterElement(UIElement)"/><br/>
    /// In this method every item should unregister their UI buffers to the page via <see cref="LegacyPage.UnregisterBuffer(UIBuffer)"/>
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
    private protected void Display(UIBuffer buffer)
    {
        if (!IsEnabled || !IsVisible)
            return;

        Terminal.Display(buffer);
    }

    /// <summary>
    /// Clear the buffer form the screen<br/>
    /// (This method calculates and displays buffers behind the one specified without displaying itself)
    /// </summary>
    /// <param name="buffer"></param>
    private protected void Clear(UIBuffer buffer)
    {
        //if (!IsEnabled || !IsVisible)   // I think this function should work anyway as it doesn't show the object itself
        //    return;

        Terminal.Display(buffer.GetGraphics(false), buffer.GetSourceArea(), buffer.Position);
    }

    protected Point LocalToScreen(Point localPoint)
        => new Point(localPoint.X + this.Position.X, localPoint.Y + this.Position.Y);

    private protected void Display(UIBuffer buffer, Point offset, Point size)
    {
        if (!IsEnabled || !IsVisible)
            return;

        Point screenPos = LocalToScreen(offset);
        Terminal.Display(buffer, new Rectangle(screenPos.X, screenPos.Y, size.X, size.Y), size);
    }

    private protected void Display(UIBuffer buffer, Rectangle screenArea)
    {
        if (!IsEnabled || !IsVisible)
            return;

        Terminal.Display(buffer, screenArea);
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
}