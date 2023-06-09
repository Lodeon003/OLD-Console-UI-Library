using System.Runtime.CompilerServices;
using System.Security.AccessControl;

namespace Lodeon.Terminal.UI;

public class TreeNavigator<T> where T : INavigable
{
    public TreeNavigator(INavigableContainer root) 
    {
        _current = root;
        _elements = root.GetChildren();
    }

    private INavigable _current;
    private object _lock = new object();
    private int _currentIndex;
    private INavigable[] _elements;
    private int _selectedIndex;
    private bool _selected = false;

    public void Previous()
    {
        lock (_lock)
        {
            // This method is safe from invalid indexes
            Navigate(_currentIndex - 1);
        }
    }
    public void Next()
    {
        lock (_lock)
        {
            // This method is safe from invalid indexes
            Navigate(_currentIndex + 1);
        }
    }

    public bool Navigate(T element) // Fai si di poter navigare a qualunque elemento. Se è un container imposta i figli,
                                    // altrimenti "selezionalo". Quando viene cambiato parent dovrebbe venir chiamato l'evento, OnNavigate sul primo figlio (per updatare le UI)
    {
        if (element == null)
            return false;

        _current = element;
    }

    public bool Navigate(int index)
    {
        lock(_lock)
        {
            if (index < 0 || index >= _elements.Length)
                return false;

            // If an element is selected and can't be deselected
            if (_selected && !Deselect())
                return false;

            _currentIndex = index;
            return true;
        }    
    }
    public bool Select(int index)
    {
        lock(_lock)
        {
            // If index is invalid
            if (index < 0 || index >= _elements.Length)
                return false;

            // If element is already selected
            if(_selected && _selectedIndex == index)
                return false;

            // If element is already selected try to deselect it and select new one
            if (_selected && !Deselect())
                return false;

            // Select
            _selected = true;
            _selectedIndex = index;
            return true;
        }
    }
    public bool Deselect()
    {
        lock(_lock)
        {
            // If already not selected
            if (!_selected)
                return false;

            // If selected element is blocking navigation
            if (_elements[_selectedIndex].IsLocked())
                return false;

            // Deselect
            _selected = false;
            return true;
        }
    }
}

public interface INavigable
{
    public bool IsLocked();
    public INavigable? GetParent();
}

public interface INavigableElement : INavigable
{
    
}

public interface INavigableContainer : INavigable
{
    public INavigable[] GetChildren();
}