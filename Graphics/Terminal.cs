using System.Drawing;
using System.Runtime.InteropServices;

namespace Lodeon.Terminal;

public static class Terminal
{
    #region Win32 Imports
    [DllImport("Kernel32")]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("Kernel32")]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [DllImport("Kernel32", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    private const int STD_OUTPUT_HANDLE = -11;
    private const uint ENABLE_PROCESSED_OUTPUT = 0x0001;
    private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
    #endregion

    private static object _lock = new object();

    private static Pixel _lastPixel = Pixel.Invisible;

    private static readonly byte _colorSequenceRed = 7, _colorSequenceGreen = 11, _colorSequenceBlue = 15;
    private static readonly char[] _foregroundSequence = "\u001b[38;2;000;000;000m".ToCharArray();
    private static readonly char[] _backgroundSequence = "\u001b[48;2;000;000;000m".ToCharArray();

    private static readonly byte _cursorSequenceX = 8, _cursorSequenceY = 2;
    private static readonly char[] _cursorPositionSequence = "\u001b[00000;00000H".ToCharArray();

    private static Color _foreground;
    private static Color _background;

    private static readonly List<char> _outputTextBuffer = new List<char>();
    //private static PixelInfo[] _outputGraphicBuffer = Array.Empty<PixelInfo>();
    public static short MaxBufferWidth { get; } = 1_000;
    public static short MaxBufferHeight { get; } = 1_000;

    public static Color Foreground
    {
        get => _foreground;

        set
        {
            if (value.Alpha == 255)
            {
                _foreground = value;
                return;
            }

            _foreground = value.Opaque();
        }
    }
    public static Color Background
    {
        get => _background;

        set
        {
            if (value.Alpha == 255)
            {
                _background = value;
                return;
            }

            _background = value.Opaque();
        }
    }

    public static bool AllowOutOfBounds { get; set; } = true;
    public static bool AllowTransparentColors { get; set; } = false;

    static Terminal()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            IntPtr stdOutHandle = GetStdHandle(STD_OUTPUT_HANDLE);
            var enabled = GetConsoleMode(stdOutHandle, out var outConsoleMode)
                && SetConsoleMode(stdOutHandle, outConsoleMode | ENABLE_PROCESSED_OUTPUT | ENABLE_VIRTUAL_TERMINAL_PROCESSING);

            if (!enabled)
                throw new InvalidOperationException("Couldn't enable terminal graphics on this windows version");
        }

        if (Environment.GetEnvironmentVariable("NO_COLOR") != null)
            throw new InvalidOperationException("Terminal graphics are disabled for this process");
    }

    /// <summary>
    /// The maximum buffer size is specified in <see cref="MaxBufferWidth"/> and <see cref="MaxBufferHeight"/> fields
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="mode"></param>
    public static void SetScreenSize(short width, short height, DisplayMode mode = DisplayMode.Medium)
    {
        short mWidth = Math.Min(width, MaxBufferWidth);
        short mHeight = Math.Min(height, MaxBufferHeight);

        switch (mode)
        {
            case DisplayMode.Monocromatic:
                _outputTextBuffer.EnsureCapacity(mWidth * mHeight + _cursorPositionSequence.Length * mHeight);
                break;

            case DisplayMode.Medium:
                _outputTextBuffer.EnsureCapacity(mWidth * mHeight * _foregroundSequence.Length + _cursorPositionSequence.Length * mHeight);
                break;

            case DisplayMode.FullColored:
                _outputTextBuffer.EnsureCapacity(mWidth * mHeight * (_foregroundSequence.Length * 2) + _cursorPositionSequence.Length * mHeight);
                break;
        }
    }

    /// <summary>
    /// Displays to the screen a graphic
    /// </summary
    /// <param name="graphic">An object that implements <see cref="IRenderable"/> interface</param>
    public static void Display(IRenderable graphic, byte colorSimilarityThreshold = 1)
    {
        lock (_lock)
        {
            // Get screen area and calculate buffer source area. Width and height are the same for source and destination,
            // but buffer starts at 0 instead of the element's screen position
            Rectangle screenArea = graphic.GetScreenArea();
            Rectangle sourceArea = new Rectangle(0, 0, screenArea.Width, screenArea.Height);

            _Display(graphic.GetGraphics(), sourceArea, new Point(screenArea.X, screenArea.Y));
        }
    }

    /// <summary>
    /// Displays to the screen a portion of a graphic
    /// </summary>
    /// <param name="graphic">An object that implements <see cref="IRenderable"/> interface</param>
    /// <param name="sourceArea">The area of the buffer array to draw.
    /// you can exclude a part of the buffer if you don't need to display the whole graphic</param>
    /// <param name="destinationPosition">The element's top-left coordinate on the screen</param>
    public static void Display(IRenderable graphic, Rectangle sourceArea, byte colorSimilarityThreshold = 1)
    {
        lock (_lock)
        {
            // Get screen area to extract element position
            Rectangle screenArea = graphic.GetScreenArea();
            _Display(graphic.GetGraphics(), sourceArea, new Point(screenArea.X, screenArea.Y));
        }
    }

    /// <summary>
    /// Displays to the screen a portion of a graphic at the specified position
    /// </summary>
    /// <param name="graphic">An object that implements <see cref="IRenderable"/> interface</param>
    /// <param name="sourceArea">The area of the buffer array to draw.
    /// you can exclude a part of the buffer if you don't need to display the whole graphic</param>
    /// <param name="destinationPosition">The element's top-left coordinate on the screen</param>
    public static void Display(IRenderable graphic, Rectangle sourceArea, Point destinationPosition, byte colorSimilarityThreshold = 1)
    {
        lock (_lock)
        {
            _Display(graphic.GetGraphics(), sourceArea, destinationPosition, colorSimilarityThreshold);
        }
    }

    /// <summary>
    /// Displays a rectangular region of pixels to the screen at the specified position
    /// </summary>
    /// <param name="buffer">An array of pixels to display</param>
    /// <param name="sourceArea">The area of the buffer to display<br/>
    /// you can exclude a part of the buffer if you don't need to display the whole graphic</param>
    /// <param name="destinationPosition">The <paramref name="buffer"/>'s top-left coordinate on the screen</param>
    public static void Display(ReadOnlySpan<Pixel> buffer, Rectangle sourceArea, Point destinationPosition, byte colorSimilarityThreshold = 1)
    {
        lock (_lock)
        {
            _Display(buffer, sourceArea, destinationPosition, colorSimilarityThreshold);
        }
    }

    /// <summary>
    /// [!] Check if window was resized form last display. If it was perform a Console.Clear()<br/>
    /// to reset the borders and maybe fire an event with current width and height and last<br/>
    /// [!] Test if <paramref name="sourceArea"/> draws pieces of buffer correctly
    /// </summary>
    private static void _Display(ReadOnlySpan<Pixel> buffer, Rectangle sourceArea, Point destinationPosition, byte colorSimilarityThreshold = 0)
    {

        if (buffer.Length == 0 || buffer.Length < sourceArea.Width * sourceArea.Height)
            return;

        // Calculate farthest screen points from the origin
        int elementLeft = destinationPosition.X;
        int elementTop = destinationPosition.Y;
        int elementRight = destinationPosition.X + sourceArea.Width;
        int elementBottom = destinationPosition.Y + sourceArea.Height;

        // Throw exception if element clips outside console buffer
        if (!AllowOutOfBounds && (elementLeft < 0 || elementTop < 0 || elementRight > Console.BufferWidth || elementBottom > Console.BufferHeight))
            throw new ArgumentException("The given coordinates are outside of the console window (buffer) bounds");

        // Remove last output
        _outputTextBuffer.Clear();

        // Calculate once and store
        short sourceRight = (short)sourceArea.Right;
        short sourceBottom = (short)sourceArea.Bottom;
        short sourceLeft = (short)sourceArea.Left;
        short sourceTop = (short)sourceArea.Top;

        for (short y = sourceTop; y < sourceBottom; y++)
        {
            // Set cursor to row's most left position
            ModifyCursorSequence(destinationPosition.X, destinationPosition.Y + y);
            _outputTextBuffer.AddRange(_cursorPositionSequence);

            // For each column in this row
            for (short x = sourceLeft; x < sourceRight; x++)
            {
                if (AllowOutOfBounds && (elementLeft + x < 0 || elementLeft + x >= Console.BufferWidth || elementTop + y < 0 || elementTop + y >= Console.BufferHeight))
                    continue;

                // Read pixel from graphic buffer
                Pixel current = buffer[x + sourceArea.Width * y];

                // Throw if semi-transparent pixels are to be printed (semi-transparent colors should be handled by user)
                if (!AllowTransparentColors && (current.Foreground.IsTransparent || current.Background.IsTransparent))
                    throw new NotImplementedException("Make sure to blend semi-transparent colors with console background");

                // Change foreground color if different from last pixel
                if (current.HasForeground && !current.Foreground.IsSimilar(_lastPixel.Foreground, colorSimilarityThreshold))
                {
                    ModifyForegroundSequence(current.Foreground);
                    _outputTextBuffer.AddRange(_foregroundSequence);
                }

                // Change background color if different from last pixel
                if (!current.Background.IsSimilar(_lastPixel.Background, colorSimilarityThreshold))
                {
                    ModifyBackgroundSequence(current.Background);
                    _outputTextBuffer.AddRange(_backgroundSequence);
                }

                // Write pixel, update last pixel
                _outputTextBuffer.Add(current.Char);
                _lastPixel = current;
            }
        }

        // Flush output buffer to the console
        Span<char> outSpan = CollectionsMarshal.AsSpan(_outputTextBuffer);
        Console.Out.Write(outSpan);
    }


    /// <summary>
    /// Clears the console buffer
    /// </summary>
    public static void Clear()
    {
        Console.Clear();
    }

    /// <summary>
    /// Clears the console buffer and changes the background color of the whole buffer
    /// </summary>
    /// <param name="background"></param>
    public static void Clear(Color background)
    {
        Pixel pixel = new Pixel().WithBackground(background);

        // Convert pixel to displayble placed at screen 0.0 coordinates
        // This changes the color of the next pixels. Clearing the console will color all console with this background
        DisplayPixel display = new DisplayPixel(pixel, Point.Empty);

        Terminal.Display(display);

        int length = Console.BufferWidth * Console.BufferHeight;
        Span<char> sp = stackalloc char[length];

        for (int i = 0; i < sp.Length; i++)
            sp[i] = ' ';

        Console.Out.Write(sp);
    }

    private static void ModifyForegroundSequence(Color color)
    => ModifyColorSequence(color.Red, color.Green, color.Blue, _foregroundSequence);

    private static void ModifyBackgroundSequence(Color color)
    => ModifyColorSequence(color.Red, color.Green, color.Blue, _backgroundSequence);

    private static void ModifyColorSequence(byte red, byte green, byte blue, char[] sequenceArray)
    {
        string redStr = red.ToString("D3");
        string blueStr = blue.ToString("D3");
        string greenStr = green.ToString("D3");

        //ToCharArray(red, sequenceArray, _colorSequenceRed);
        //ToCharArray(green, sequenceArray, _colorSequenceBlue);
        //ToCharArray(blue, sequenceArray, _colorSequenceGreen);

        for (int i = 0; i < 3; i++)
        {
            sequenceArray[_colorSequenceRed + i] = redStr[i];
            sequenceArray[_colorSequenceBlue + i] = blueStr[i];
            sequenceArray[_colorSequenceGreen + i] = greenStr[i];
        }
    }

    private static void ModifyCursorSequence(int x, int y)
    {
        if (x < 0 || y < 0 || x > 99999 || y > 99999)
            throw new NotImplementedException("The array allocates 5 chars for each coordinate");
        // Console index starts at 0;0 but ANSI cursor position start from 1;1
        string xStr = (x + 1).ToString("D5");
        string yStr = (y + 1).ToString("D5");

        //ToCharArray((uint)x + 1, _cursorPositionSequence, _cursorSequenceX);
        //ToCharArray((uint)y + 1, _cursorPositionSequence, _cursorSequenceY);

        for (int i = 0; i < 5; i++)
        {
            _cursorPositionSequence[_cursorSequenceX + i] = xStr[i];
            _cursorPositionSequence[_cursorSequenceY + i] = yStr[i];
        }
    }

    public static int ToCharArray(uint value, Span<char> buffer, int bufferIndex)
    {
        if (value == 0)
        {
            buffer[bufferIndex] = '0';
            return 1;
        }

        int len = 1;
        for (uint rem = value / 10; rem > 0; rem /= 10)
            len++;

        // Throw if number is too big

        for (int i = len - 1; i >= 0; i--)
        {
            buffer[bufferIndex + i] = (char)('0' + (value % 10));
            value /= 10;
        }
        return len;
    }
}