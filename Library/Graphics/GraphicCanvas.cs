namespace Lodeon.Terminal.Graphics
{
    public class GraphicCanvas
    {
        private GraphicBuffer _buffer;

        public GraphicCanvas(GraphicBuffer buffer)
        {
            _buffer = buffer;
        }

        public void Fill(Color color)
            => _buffer.Fill(new Pixel().WithBackground(color));

        public ReadonlyGraphicBuffer AsReadonly()
        {
            return new ReadonlyGraphicBuffer(_buffer);
        }
    }
}