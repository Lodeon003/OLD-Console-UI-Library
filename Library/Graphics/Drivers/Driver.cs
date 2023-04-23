using Lodeon.Terminal;
using System;
using Rectangle = System.Drawing.Rectangle;
using Point = System.Drawing.Point;
using Lodeon.Terminal.Graphics.Drivers;

/// <summary>
/// Base class for graphic drivers. A graphic driver is used by the library as an interface to display graphics. <br/>
/// Default drivers for the <see cref="Console"/> are present in this library. This class can be extended to redirect graphic output to
/// another source.<br/>
/// This class implements a static lock to make sure that only one instance of <see cref="Driver"/> at the time can call <see cref="Display(System.ReadOnlySpan{Lodeon.Terminal.Pixel}, System.Drawing.Rectangle, System.Drawing.Point)"/>
/// </summary>
public abstract class Driver : IDisposable
{
    public delegate void WindowResizedDel(Rectangle lastSize, Rectangle newSize);
    public abstract event WindowResizedDel? WindowResized;

    public delegate void ConsoleInputDel(ConsoleKeyInfo keyInfo);
    public abstract event ConsoleInputDel? KeyboardInputDown;
    public abstract event ConsoleInputDel? KeyboardInputUp;

    public delegate void MouseInputDel(int button, Point position);
    public abstract event MouseInputDel? MouseInputDown;
    public abstract event MouseInputDel? MouseInputUp;

    private static object _lock = new object();
    public abstract int ScreenWidth { get; }
    public abstract int ScreenHeight { get; }
    protected bool Disposed { get; private set; } = false;

    /// <summary>
    /// Creates a new instance of this <see cref="Driver"/> class<br/>
    /// <br/>
    /// - An excpetion will be thrown if colors are disabled on this terminal. There is no uniform way to check, this method
    /// checks for any envirorment variable called "NO_COLOR"<br/>
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public Driver()
    {
        if (Environment.GetEnvironmentVariable("NO_COLOR") != null)
            throw new InvalidOperationException("Terminal graphics are disabled for this process");
    }

    /// <summary>
    /// Displays to the screen a graphic
    /// </summary
    /// <param name="graphic">An object that implements <see cref="IRenderable"/> interface</param>
    public void Display(IRenderable graphic)
    {
        if (Disposed)
            throw new ObjectDisposedException(this.GetType().FullName);

        // Get screen area and calculate buffer source area. Width and height are the same for source and destination,
        // but buffer starts at 0 instead of the element's screen position
        Rectangle screenArea = graphic.GetScreenArea();
        Rectangle sourceArea = new Rectangle(0, 0, screenArea.Width, screenArea.Height);

        lock (_lock)
        {
            OnDisplay(graphic.GetGraphics(), sourceArea, new Point(screenArea.X, screenArea.Y));
        }
    }

    /// <summary>
    /// Displays to the screen a portion of a graphic
    /// </summary>
    /// <param name="graphic">An object that implements <see cref="IRenderable"/> interface</param>
    /// <param name="sourceArea">The area of the buffer array to draw.
    /// you can exclude a part of the buffer if you don't need to display the whole graphic</param>
    /// <param name="destinationPosition">The element's top-left coordinate on the screen</param>
    public void Display(IRenderable graphic, Rectangle sourceArea)
    {
        if (Disposed)
            throw new ObjectDisposedException(this.GetType().FullName);

        // Get screen area to extract element position
        Rectangle screenArea = graphic.GetScreenArea();
        
        lock (_lock)
        {
            OnDisplay(graphic.GetGraphics(), sourceArea, new Point(screenArea.X, screenArea.Y));
        }
    }

    /// <summary>
    /// Displays to the screen a portion of a graphic at the specified position
    /// </summary>
    /// <param name="graphic">An object that implements <see cref="IRenderable"/> interface</param>
    /// <param name="sourceArea">The area of the buffer array to draw.
    /// you can exclude a part of the buffer if you don't need to display the whole graphic</param>
    /// <param name="destinationPosition">The element's top-left coordinate on the screen</param>
    public void Display(IRenderable graphic, Rectangle sourceArea, Point destinationPosition)
    {
        if (Disposed)
            throw new ObjectDisposedException(this.GetType().FullName);

        lock (_lock)
        {
            OnDisplay(graphic.GetGraphics(), sourceArea, destinationPosition);
        }
    }

    /// <summary>
    /// add description
    /// </summary>
    /// <param name="graphic">An object that implements <see cref="IRenderable"/> interface</param>
    /// <param name="sourceArea">The area of the buffer array to draw.
    /// you can exclude a part of the buffer if you don't need to display the whole graphic</param>
    /// <param name="destinationPosition">The element's top-left coordinate on the screen</param>
    public void Display(ReadOnlySpan<Pixel> buffer, Rectangle sourceArea, Point destinationPosition)
    {
        if (Disposed)
            throw new ObjectDisposedException(this.GetType().FullName);

        lock (_lock)
        {
            OnDisplay(buffer, sourceArea, destinationPosition);
        }
    }

    /// <summary>
    /// Displays a row of characters starting from a certain point on the screen
    /// </summary>
    /// <param name="characters"></param>
    public void Display(Span<char> characters, Point destinationPosition)
    {
        if (Disposed)
            throw new ObjectDisposedException(this.GetType().FullName);

        lock (_lock)
        {
            OnDisplay(characters, destinationPosition);
        }
    }
    public void Dispose()
    {
        if (Disposed)
            return;

        OnDispose();

        Disposed = true;
    }

    protected abstract void OnDisplay(ReadOnlySpan<Pixel> buffer, Rectangle sourceArea, Point destinationPosition);
    protected abstract void OnDisplay(ReadOnlySpan<char> buffer, Point destinationPosition);

    /// <summary>
    /// Clears the console buffer
    /// </summary>
    public abstract void Clear();
    protected abstract void OnDispose();

    internal static Driver GetDefaultDriver()
    {
        if(OperatingSystem.IsWindows())
        {
            return new WindowsDriver();
        }
        return new AnsiDriver();
    }

    public abstract void SetBackground(Color background);
    public abstract void SetForeground(Color foreground);
}