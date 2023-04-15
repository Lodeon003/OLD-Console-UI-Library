using System.Drawing;
using System.Runtime.InteropServices;

namespace Lodeon.Terminal
{
    /// <summary>
    /// <see cref="Pixel"/> struct that can be directly displayed to <see cref="AnsiDriver"/> (Implements <see cref="IRenderable"/>)<br/>
    /// Use not recommended
    /// </summary>
    public struct DisplayPixel : IRenderable
    {
        private Point _position;
        private Pixel _pixel;

        public DisplayPixel(Pixel pixel, Point position)
        {
            _pixel = pixel;
            _position = position;
        }

        public ReadOnlySpan<Pixel> GetGraphics()
            => MemoryMarshal.CreateSpan(ref _pixel, 1);

        public Rectangle GetScreenArea()
            => new Rectangle(_position.X, _position.Y, 1, 1);

        public static DisplayPixel Create(Pixel pixel, Point screenPosition)
            => new DisplayPixel(pixel, screenPosition);
    }
}
