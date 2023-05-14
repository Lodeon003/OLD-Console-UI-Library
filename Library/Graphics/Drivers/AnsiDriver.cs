using System.Buffers;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Lodeon.Terminal.Graphics.Drivers;

public sealed class AnsiDriver : Driver
{
    private Pixel _lastPixel = Pixel.Invisible;

    private readonly byte _colorSequenceRed = 7, _colorSequenceGreen = 11, _colorSequenceBlue = 15;
    private readonly char[] _foregroundSequence = "\u001b[38;2;000;000;000m".ToCharArray();
    private readonly char[] _backgroundSequence = "\u001b[48;2;000;000;000m".ToCharArray();

    private readonly byte _cursorSequenceX = 8, _cursorSequenceY = 2;
    private readonly char[] _cursorPositionSequence = "\u001b[00000;00000H".ToCharArray();

    private readonly List<char> _outputTextBuffer = new List<char>();
    private Color _foreground;
    private Color _background;

    private CancellationTokenSource _disposeToken = new CancellationTokenSource();

    public override event WindowResizedDel? WindowResized;
    public override event ConsoleInputDel? KeyboardInputDown;
    public override event ConsoleInputDel? KeyboardInputUp;
    public override event MouseInputDel? MouseInputDown;
    public override event MouseInputDel? MouseInputUp;

    public short MaxBufferWidth { get; } = 1_000;
    public short MaxBufferHeight { get; } = 1_000;
    public Color Foreground
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
    public Color Background
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

    public override int ScreenWidth => _screenWidth;
    public override int ScreenHeight => _screenHeight;
    public byte ColorSimilarityThreshold { get; private init; }

    private int _lastWidth = 0;
    private int _lastHeight = 0;
    private int _screenWidth = 0;
    private int _screenHeight = 0;

    /// <summary>
    /// <inheritdoc/>
    /// - An exception will be thrown if this is a windows process and can't enable "ENABLE_VIRTUAL_TERMINAL_PROCESSING"
    /// and "ENABLE_PROCESSED_OUTPUT" via "SetConsoleMode" in "Kernel32"
    /// </summary>
    /// <exception cref="DriverException"></exception>
    public AnsiDriver(byte colorSimilarityThreshold = 0) : base()
    {
        ColorSimilarityThreshold = colorSimilarityThreshold;
        
        Setup();
        UpdateWindowSize();

        Task.Run(Input);
    }

    private void Setup()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            IntPtr stdOutHandle = WindowsNative.GetStdHandle(WindowsNative.STD_OUTPUT_HANDLE);
            var enabled = WindowsNative.GetConsoleMode(stdOutHandle, out var outConsoleMode)
                && WindowsNative.SetConsoleMode(stdOutHandle, outConsoleMode | WindowsNative.ENABLE_PROCESSED_OUTPUT | WindowsNative.ENABLE_VIRTUAL_TERMINAL_PROCESSING);

            if (!enabled)
                throw new DriverException("Couldn't enable terminal graphics on this windows version");
        }
    }

    private async Task Input()
    {
        try
        {
            await Task.Run(InputLoop).WaitAsync(_disposeToken.Token);
        }
        catch(OperationCanceledException)
        { }
    }

    private void InputLoop()
    {
        while(true)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            
            InvokeKeyboardInput(keyInfo);
            KeyboardInputDown?.Invoke(keyInfo);
            KeyboardInputUp?.Invoke(keyInfo);
        }
    }

    private void UpdateWindowSize()
    {
        _lastWidth = _screenWidth;
        _lastHeight = _screenHeight;
        _screenWidth = Math.Min(Console.WindowWidth, Console.BufferWidth);
        _screenHeight = Math.Min(Console.BufferHeight, Console.WindowHeight);
    }

    private void InvokeWindowIfResize()
    {
        if (_lastWidth == _screenWidth && _lastHeight == _screenWidth)
            return;

        WindowResized?.Invoke(new(_lastWidth, _lastHeight), new(_screenWidth, _screenHeight));
    }

    private void ModifyForegroundSequence(Color color)
        => ModifyColorSequence(color.Red, color.Green, color.Blue, _foregroundSequence);

    private void ModifyBackgroundSequence(Color color)
        => ModifyColorSequence(color.Red, color.Green, color.Blue, _backgroundSequence);

    private void ModifyColorSequence(byte red, byte green, byte blue, char[] sequenceArray)
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
    private void ModifyCursorSequence(int x, int y)
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


    /// <summary>
    /// To test! Make it so number pads to the right and not to the left if it doesn't occupy all space available.<br/>
    /// Implement in function <see cref="ModifyCursorSequence(int, int)"/> <see cref="ModifyColorSequence(byte, byte, byte, char[])"/>
    /// and <see cref="ModifyBackgroundSequence(Color)"/> replacing the "ToString".
    /// </summary>
    private int ToCharArray(uint value, Span<char> buffer, int bufferIndex)
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
            buffer[bufferIndex + i] = (char)('0' + value % 10);
            value /= 10;
        }
        return len;
    }


    #region Class Overrides
    /// <summary>
    /// [!] Check if window was resized form last display. If it was perform a Console.Clear()<br/>
    /// to reset the borders and maybe fire an event with current width and height and last<br/>
    /// [!] Test if <paramref name="sourceArea"/> draws pieces of buffer correctly
    /// </summary>
    protected sealed override void OnDisplay(ReadOnlySpan<Pixel> buffer, Rectangle sourceArea, Point destinationPosition)
    {
        UpdateWindowSize();
        
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
                if (!AllowTransparentColors && (current.Foreground.IsSemiTransparent || current.Background.IsSemiTransparent))
                    throw new NotImplementedException("Make sure to blend semi-transparent colors with console background");

                // Change foreground color if different from last pixel
                if (current.HasForeground && !current.Foreground.IsSimilar(_lastPixel.Foreground, ColorSimilarityThreshold))
                {
                    ModifyForegroundSequence(current.Foreground);
                    _outputTextBuffer.AddRange(_foregroundSequence);
                }

                // Change background color if different from last pixel
                if (!current.Background.IsSimilar(_lastPixel.Background, ColorSimilarityThreshold))
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

        InvokeWindowIfResize();
    }

    protected override void OnDisplay(ReadOnlySpan<char> buffer, Point destinationPosition)
    {
        Console.SetCursorPosition(destinationPosition.X, destinationPosition.Y);
        Console.Out.Write(buffer);
    }

    protected override void OnClear()
        => OnClear(Color.Black);

    protected override void OnClear(Color background)
    {
        UpdateWindowSize();
        
        int length = Console.WindowWidth * Console.WindowHeight;

        Span<char> sp = ArrayPool<char>.Shared.Rent(length).AsSpan();
        sp = sp.Slice(0, length);

        for (int i = 0; i < length; i++)
            sp[i] = ' ';

        OnSetBackground(background.Opaque());
        Display(sp, Point.Empty);
        
        InvokeWindowIfResize();
    }

    protected override void OnSetBackground(Color background)
    {
        UpdateWindowSize();

        Pixel pixel = new Pixel().WithBackground(background);

        Span<Pixel> span = MemoryMarshal.CreateSpan(ref pixel, 1);
        Display(span, new Rectangle(0, 0, 1, 1), Point.Empty);

        InvokeWindowIfResize();
    }

    protected override void OnSetForeground(Color foreground)
    {
        UpdateWindowSize();

        Pixel pixel = new Pixel().WithForeground(foreground);

        Span<Pixel> span = MemoryMarshal.CreateSpan(ref pixel, 1);
        Display(span, new Rectangle(0, 0, 1, 1), Point.Empty);

        InvokeWindowIfResize();
    }

    protected override void OnDispose()
    {
        _disposeToken?.Dispose();
    }
    #endregion
}