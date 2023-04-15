namespace Lodeon.Terminal.UI;

public class PageInitializer
{
    private Dictionary<string, Page> _dictionary;
    public PageInitializer(Dictionary<string, Page> dictionary)
    {
        _dictionary = dictionary;
    }

    public void Add<TPage>(string name) where TPage : Page, new()
        => Add<TPage>(name, _dictionary.Count == 0);

    public void Add<TPage>(string name, bool isMain) where TPage : Page, new()
    {
        TPage page = new TPage();

        if (isMain)
            page.SetMain();

        if (!_dictionary.TryAdd(name, page))
            throw new Exception("A value was already added with same key");
    }
}
