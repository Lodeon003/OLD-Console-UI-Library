using System.Drawing;

namespace Lodeon.Terminal.Graphics
{
    public class ReadonlyGraphicBuffer : IRenderable
    {
        private GraphicBuffer _buffer;

        public ReadonlyGraphicBuffer(GraphicBuffer buffer)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            _buffer = buffer;
        }

        public Pixel this[int index]
        => _buffer[index];

        public Pixel this[short x, short y]
            => _buffer[x, y];

        public ReadOnlySpan<Pixel> GetGraphics()
            => _buffer.GetGraphics();

        public Rectangle GetScreenArea()
            => _buffer.GetScreenArea();
    }
}