using System.Drawing;

namespace Lodeon.Terminal.Graphics
{
    public class ReadonlyGraphicBuffer
    {
        private GraphicBuffer _buffer;

        public int Length => _buffer.Length;

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
    }
}