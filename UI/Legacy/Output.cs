using Lodeon.Terminal.Graphics.Drivers;

namespace Lodeon.Terminal.UI
{
    public static class Output
    {
        public static LegacyPage? CurrentPage { get; private set; }

        public delegate void PageChanged(LegacyPage newPage);
        public static event PageChanged? OnPageChanged;

        public static void SetPage(LegacyPage page)
        {
            if (page == null || page == CurrentPage)
                return;

            // Set background as page's background
            AnsiDriver.Clear(page.Background);

            OnPageChanged?.Invoke(page);
            CurrentPage = page;
        }
    }
}
