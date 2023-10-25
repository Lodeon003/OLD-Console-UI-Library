using System.Runtime.CompilerServices;
using System.Security.AccessControl;

namespace Lodeon.Terminal.UI.Navigation;

// Fai di salvare tutto con le interfacce ma controllando che solo valori che derivano da T vengano inseriti. Converti all'uscita T

public class TreeNavigator
{
    public TreeNavigator(IContainer root)
    {
        SetContainer(root);
    }

    public delegate void GenericDel(INavigable element);
    public delegate void ElementDel(INavigableElement element);
    public delegate void GenericNullableDel(INavigable? element);
    public delegate void ContainerDel(INavigableContainer element);

    //public event ContainerDel? OnTreeChangedFrom;
    //public event ContainerDel? OnTreeChangedTo;
    //public event GenericNullableDel? OnNavigateFrom;
    public event GenericDel? OnNavigate;
    public event ElementDel? Focus;
    public event ElementDel? Unfocus;

    private object _lock = new object();
    private IContainer _currentContainer;
    private int _currentIndex;
    private bool _isFocused = false;

    /// <summary>
    /// Tries to navigate to the next child of this container
    /// </summary>
    public bool Previous()
    {
        lock (_lock)
        {
            // This method is safe from invalid indexes
            return Navigate(_currentIndex - 1);
        }
    }

    /// <summary>
    /// Tries to navigate to the next child of this container
    /// </summary>
    public void Next()
    {
        lock (_lock)
        {
            // This method is safe from invalid indexes
            Navigate(_currentIndex + 1);
        }
    }

    public bool NavigateIn()
    {
        IElement? child = _currentContainer.GetChildren(_currentIndex);

        if(child is null)
            return false;

        // If element is 
        if (_isFocused && !Unfocus())
            return false;

        // If it is a container set it as current container
        if (currentElement is INavigableContainer container)
            return SetContainer(container);

        // Else select element
        return Focus(_currentIndex);
    }

    public bool NavigateOut()
    {
        // If an element is selected, try to deselect it
        if (_isFocused && !Unfocus())
            return false;

        // If no element is selected try to move out to parent
        IContainer? parent = _currentContainer.Parent;

        if (parent == null)
            return false;

        // If it is a container set it as current container
        SetContainer(parent);
        return true;
    }

    public bool SetContainer(IContainer element) // Fai si di poter navigare a qualunque elemento. Se è un container imposta i figli, // altrimenti "selezionalo". Quando viene cambiato parent dovrebbe venir chiamato l'evento, OnNavigate sul primo figlio (per updatare le UI)
    {
        lock (_lock)
        {
            if (element == null)
                return false;

            // Unfocus element if one is selected
            if (_isFocused && !Unfocus())
                return false;

            _currentContainer = element;
            _currentIndex = 0;
            _isFocused = false;
            return true;
        }
    }


    /// <summary>
    /// Returns true if no element was selected or an element was selected and got deselected
    /// </summary>
    /// <returns></returns>
    private bool TryUnfocus()
    {
        lock (_lock)
        {
            // If no element is selected action succeded
            if (!_isElementSelected)
                return true;

            // If element is selected try to deselect it
            return Unfocus();
        }
    }

    /// <summary>
    /// Navigates to a specific child in the current container
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool Navigate(int index)
    {
        lock (_lock)
        {
            if (index < 0 || index >= _elements.Length)
                return false;

            // Try to deselect an element if selected
            if (!TryUnfocus())
                return false;

            INavigable lastElement = _elements[_currentIndex];
            INavigable currentElement = _elements[index];

            _currentIndex = index;

            OnNavigateFrom?.Invoke(lastElement);
            OnNavigateTo?.Invoke(child);
            return true;
        }
    }

    /// <summary>
    /// Tries to select an element and returns true if operation succeded. If the element is a container the tree will be updated
    /// It retuns false if
    /// <list type="bullet">
    /// <item>Index was invalid</item>
    /// <item>Element at <paramref name="index"/> is already selected</item>
    /// <item>The element was a container</item>
    /// <item>An element is locking navigation</item>
    /// </list>
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool Focus(int index)
    {
        lock (_lock)
        {
            // If index is invalid
            if (index < 0 || index >= _elements.Length)
                return false;

            // If element is container
            if (_elements[_selectedIndex] is not INavigableElement)
                return false;

            // If element is already selected
            if (_isElementSelected && _selectedIndex == index)
                return false;

            // If element is already selected try to deselect it and select new one
            if (!TryUnfocus())
                return false;

            // Focus
            OnSelect?.Invoke((INavigableElement)_elements[_selectedIndex]);
            _isFocused = true;
            _selectedIndex = index;
            return true;
        }
    }

    /// <summary>
    /// Returns true if an element was selected and got deselected.<br/>
    /// Returns false if
    /// <list type="bullet">
    /// <item>No element was selected</item>
    /// <item>The selected element is locking navigation</item>
    /// </list>
    /// </summary>
    /// <returns></returns>
    public bool Unfocus()
    {
        lock (_lock)
        {
            // If already not selected
            if (!_isFocused)
                return false;

            // If selected element is blocking navigation
            if (_elements[_selectedIndex].IsLocked())
                return false;

            // Unfocus
            _isFocused = false;
            OnDeselect?.Invoke((INavigableElement)_elements[_selectedIndex]);
            return true;
        }
    }
}

public interface INavigable
{
    public bool IsLocked();
    public INavigableContainer? GetParent();
}

public interface INavigableElement : INavigable
{

}

public interface INavigableContainer : INavigable
{
    public INavigable[] GetChildren();
}