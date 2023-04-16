namespace Lodeon.Terminal.UI;

/// <summary>
/// A class that initializes a collection of UI pages. It interfaces with users making them able to choose which page will be the main one
/// </summary>
public class PageInitializer
{
    private readonly Dictionary<string, Page> _dictionary;
    private readonly Driver _driver;
    private bool _wasMain = false;

    public PageInitializer(Dictionary<string, Page> dictionary, Driver driver)
    {
        _dictionary = dictionary;
        _driver = driver;
    }

    public void Add<TPage>(string name) where TPage : Page, new()
        => Add<TPage>(name, _dictionary.Count == 0);

    public void Add<TPage>(string name, bool isMain) where TPage : Page, new()
    {
        TPage page = new TPage();

        if (!_wasMain && isMain)
            _wasMain = true;
        else
            throw new InvalidOperationException("More than one page was set as Main page");

        page.Initialize(_driver, isMain);

        if (!_dictionary.TryAdd(name, page))
            throw new Exception("A value was already added with same key");
    }
}
