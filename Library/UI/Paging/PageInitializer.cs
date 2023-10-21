using System.Security;
using Lodeon.Terminal.UI.Navigation;

namespace Lodeon.Terminal.UI.Paging;

/// <summary>
/// A class that initializes a collection of UI pages. It interfaces with users making them able to choose which page will be the main one
/// </summary>
public class PageInitializer
{
    private readonly Dictionary<string, Page> _dictionary;
    private Page? _mainPage;
    private GraphicBuffer _screenBuffer;
    private Script _program;
    private ExceptionHandler _handler;
    private Navigator<string, Page> _navigator;

    public Page Main => _mainPage ?? throw new ArgumentException($"No page was initialized as main. ONe must be initialized using {nameof(AddMain)} method");

    public PageInitializer(Dictionary<string, Page> dictionary, Navigator<string, Page> navigator, GraphicBuffer screenBuffer, Script program, ExceptionHandler handler)
    {
        _dictionary = dictionary;
        _screenBuffer = screenBuffer;
        _program = program;
        _handler = handler;
        _navigator = navigator;
    }

    public void AddMain<TPage>(string name) where TPage : Page, new()
        => Add<TPage>(name, true);

    public void Add<TPage>(string name) where TPage : Page, new()
        => Add<TPage>(name, false);

    private void Add<TPage>(string name, bool isMain) where TPage : Page, new()
    {
        TPage page = new TPage();

        if (_mainPage is null)
            _mainPage = page;
        else
            throw new ArgumentException("More than one page was set as Main page", nameof(isMain));

        page.Initialize(_program, isMain, _screenBuffer, _handler, _navigator);

        if (!_dictionary.TryAdd(name, page))
            throw new ArgumentException($"More than one page have the same name: \"{name}\"", nameof(name));
    }
}
