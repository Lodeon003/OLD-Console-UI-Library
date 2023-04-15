namespace Lodeon.Terminal.UI.Legacy.BasicElements
{
    public class BasicLabel : UIElement
    {
        public Color Background { get; set; } = Color.Invisible;
        public Color Foreground { get; set; } = Color.White;

        private string _text = string.Empty;
        private UIBuffer _buffer;

        public BasicLabel(LegacyPage page, string text) : base(page)
        {
            _text = text;
            _buffer = new UIBuffer(this, Width, Height);
            UpdateContent();
        }

        public BasicLabel(LegacyPage page) : base(page)
        {
            _buffer = new UIBuffer(this, Width, Height);
            UpdateContent();
        }

        public void SetText(string text)
        {
            _text = text;
            UpdateContent();
            Display(_buffer);
        }

        internal override void OnRegister()
        {
            Page.RegisterBuffer(_buffer);
        }

        internal override void OnUnregister()
        {
            Page.UnregisterBuffer(_buffer);
        }

        protected override void OnBeforeChangePosition()
        {
            Clear(_buffer);
        }

        protected override void OnChangePosition()
        {
            _buffer.SetPosition(Position);
            Display(_buffer);
        }

        protected override void OnChangeSize()
        {
            _buffer.Resize(Width, Height);
            UpdateContent();
            Display(_buffer);
        }

        private void UpdateContent()
        {
            int xMax = Math.Min(_text.Length, Width);

            for (int x = 0; x < xMax; x++)
                _buffer.Write(x, new Pixel(_text[x], Foreground, Background));

            // Fill rest of line with nothing
            for (int i = xMax; i < Width; i++)
                _buffer.Write(i, new Pixel(Pixel.EmptyCharacter, Foreground, Background));
        }

        protected override void OnEnable()
        {
            _buffer.Resize(Width, Height);
            UpdateContent();
            Display(_buffer);
        }
    }
}
