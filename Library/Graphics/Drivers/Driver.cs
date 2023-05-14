using Lodeon.Terminal;
using System;
using Rectangle = System.Drawing.Rectangle;
using Point = System.Drawing.Point;
using Lodeon.Terminal.Graphics.Drivers;
using Lodeon.Terminal.UI.Units;

/// <summary>
/// Base class for graphic drivers. A graphic driver is used by the library as an interface to display graphics. <br/>
/// Default drivers for the <see cref="Console"/> are present in this library. This class can be extended to redirect graphic output to
/// another source.<br/>
/// All methods and callbacks are thread safe via LOCK implementation. Lock is static, only ONE instance of theese classes can write to the console at once
/// </summary>
public abstract class Driver : IDisposable
{
    public delegate void WindowResizedDel(PixelPoint lastSize, PixelPoint newSize);
    public abstract event WindowResizedDel? WindowResized;

    public delegate void ConsoleInputDel(ConsoleKeyInfo keyInfo);
    public abstract event ConsoleInputDel? KeyboardInputDown;
    public abstract event ConsoleInputDel? KeyboardInputUp;

    public delegate void MouseInputDel(int button, PixelPoint position);
    public abstract event MouseInputDel? MouseInputDown;
    public abstract event MouseInputDel? MouseInputUp;

    private static readonly object _lock = new object();

    public bool AllowOutOfBounds { get; set; } = true;
    public bool AllowTransparentColors { get; set; } = false;

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
    /// Returns a driver that bests fits the current operating system
    /// </summary>
    /// <returns>An instance of a Driver class</returns>
    /// <exception cref="DriverException"/>
    public static Driver GetDefaultDriver()
    {
        if (OperatingSystem.IsWindows())
        {
            return new WindowsDriver();
        }
        return new AnsiDriver();
    }


    #region PUBLIC METHODS

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
    public void Display(ReadOnlySpan<Pixel> buffer, Rectangle sourceArea, PixelPoint destinationPosition)
    {
        if (Disposed)
            throw new ObjectDisposedException(this.GetType().FullName);

        lock (_lock)
        {
            sourceArea = sourceArea.Clamp(new(0, 0, ScreenWidth, ScreenHeight));
            destinationPosition = PixelPoint.Clamp(destinationPosition, new(0, 0), new(ScreenWidth, ScreenHeight));
            OnDisplay(buffer, sourceArea, destinationPosition);
        }
    }


    /// <summary>
    /// Displays a row of characters starting from a certain point on the screen
    /// </summary>
    /// <param name="characters"></param>
    public void Display(ReadOnlySpan<char> characters, Point destinationPosition)
    {
        if (Disposed)
            throw new ObjectDisposedException(this.GetType().FullName);

        lock (_lock)
        {
            OnDisplay(characters, destinationPosition);
        }
    }
    

    /// <summary>
    /// IDisposable implementation. Call to release all unmanaged resources
    /// </summary>
    public void Dispose()
    {
        if (Disposed)
            return;

        lock (_lock)
        {
            OnDispose();
            Disposed = true;
        }
    }


    /// <summary>
    /// Clears the console buffer
    /// </summary>
    public void Clear()
    {
        lock(_lock)
        {
            OnClear();
        }
    }
    

    /// <summary>
    /// Clears the console buffer and changes the background color of the whole buffer
    /// </summary>
    public void Clear(Color background)
    {
        lock(_lock)
        {
            OnClear(background);
        }
    }


    // Added to change color to Display(Span<char>) method
    /// <summary>
    /// Set background color of the console's cursor
    /// </summary>
    /// <param name="background">The color to set</param>
    public void SetBackground(Color background)
    {
        lock(_lock)
        {
            OnSetBackground(background);
        }
    }

    /// <summary>
    /// Set foreground color of the console's cursor
    /// </summary>
    /// <param name="foreground">The color to set</param>
    public void SetForeground(Color foreground)
    {
        lock(_lock)
        {
            OnSetForeground(foreground);
        }
    }
    #endregion

    #region Protected Callbacks
    /// <summary>
    /// Implementation: show specified data to the console screen
    /// </summary>
    /// <param name="buffer">Memory representing an array of <see cref="Pixel"/>s to display</param>
    /// <param name="sourceArea">The area of the array where to take pixels from</param>
    /// <param name="destinationPosition">Where to display the array on screen</param>
    protected abstract void OnDisplay(ReadOnlySpan<Pixel> buffer, Rectangle sourceArea, Point destinationPosition);


    /// <summary>
    /// Implementation: display specified text on screen at specified destination
    /// </summary>
    protected abstract void OnDisplay(ReadOnlySpan<char> buffer, Point destinationPosition);


    /// <summary>
    /// Implementation: set foreground color of console's cursor
    /// </summary>
    protected abstract void OnSetForeground(Color foreground);

    /// <summary>
    /// Implementation: set background color of console's cursor
    /// </summary>
    protected abstract void OnSetBackground(Color background);

    /// <summary>
    /// Implementation: delete all text visible on screen
    /// </summary>
    protected abstract void OnClear();


    /// <summary>
    /// Implementation: Delete all text visible on screen and change background to the specified color
    /// </summary>
    protected abstract void OnClear(Color background);


    /// <summary>
    /// Callback raised when <see cref="Dispose"/> method gets called on this object and it hasn't been desposed yet
    /// </summary>
    protected abstract void OnDispose();


    // [!] Make thread safe
    protected void InvokeKeyboardInput(ConsoleKeyInfo keyInfo)
    {
        _lastKey = keyInfo;
        _semaphore.Release();
    }

    public ConsoleKeyInfo GetKeyDown()
    {
        _semaphore.Wait();
        return _lastKey;
    }

    private ConsoleKeyInfo _lastKey;
    private SemaphoreSlim _semaphore = new SemaphoreSlim(0);
    #endregion
}