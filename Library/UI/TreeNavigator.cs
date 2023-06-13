using System.Runtime.CompilerServices;
using System.Security.AccessControl;

namespace Lodeon.Terminal.UI;

// Fai di salvare tutto con le interfacce ma controllando che solo valori che derivano da T vengano inseriti. Converti all'uscita T

public class TreeNavigator//<T> where T : INavigable
{
    public TreeNavigator(INavigableContainer root)
    {
        SetContainer(root);
    }

    public delegate void GenericDel(INavigable element);
    public delegate void ElementDel(INavigableElement element);
    public delegate void GenericNullableDel(INavigable? element);
    public delegate void ContainerDel(INavigableContainer element);

    public event ContainerDel? OnTreeChangedFrom;
    public event ContainerDel? OnTreeChangedTo;
    public event GenericNullableDel? OnNavigateFrom;
    public event GenericDel? OnNavigateTo;
    public event ElementDel? OnSelect;
    public event ElementDel? OnDeselect;

    private object _lock = new object();
    private INavigableContainer _currentContainer;
    private INavigable[] _elements;
    private int _currentIndex;
    private int _selectedIndex;
    private bool _isElementSelected = false;

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
        INavigable currentElement = _elements[_currentIndex];

        // If it is a container set it as current container
        if(currentElement is INavigableContainer container)
            return SetContainer(container);

        // Else select element
        return Select(_currentIndex);
    }

    public bool NavigateOut()
    {
        // If an element is selected, try to deselect it
        if (Deselect())
            return true;

        // If no element is selected try to move out to parent
        INavigableContainer? parent = _currentContainer.GetParent();

        if (parent == null)
            return false;

        // If an element is selected deselect it
        if (_isElementSelected)
            return Deselect();

        // If it is a container set it as current container
        return SetContainer(parent);
    }

    public bool SetContainer(INavigableContainer element) // Fai si di poter navigare a qualunque elemento. Se è un container imposta i figli, // altrimenti "selezionalo". Quando viene cambiato parent dovrebbe venir chiamato l'evento, OnNavigate sul primo figlio (per updatare le UI)
    {
        lock(_lock)
        {            
            if (element == null)
                return false;

            // Deselect element if one is selected
            if (!TryDeselect())
                return false;

            _elements = element.GetChildren();

            INavigableContainer lastContainer = _currentContainer;
            INavigableContainer newContainer = element;

            OnTreeChangedFrom?.Invoke(lastContainer);
            OnTreeChangedTo?.Invoke(newContainer);

            // Update tree data
            _currentContainer = newContainer;
            _elements = element.GetChildren();
            Navigate(0);

            return true;
        }
    }


    /// <summary>
    /// Returns true if no element was selected or an element was selected and got deselected
    /// </summary>
    /// <returns></returns>
    private bool TryDeselect()
    {
        lock(_lock)
        {
            // If no element is selected action succeded
            if (!_isElementSelected) 
                return true;

            // If element is selected try to deselect it
            return Deselect();
        }
    }

    /// <summary>
    /// Navigates to a specific child in the current container
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool Navigate(int index)
    {
        lock(_lock)
        {
            if (index < 0 || index >= _elements.Length)
                return false;

            // Try to deselect an element if selected
            if (!TryDeselect())
                return false;

            INavigable lastElement = _elements[_currentIndex];
            INavigable currentElement = _elements[index];
            
            _currentIndex = index;

            OnNavigateFrom?.Invoke(lastElement);
            OnNavigateTo?.Invoke(currentElement);
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
    public bool Select(int index)
    {
        lock(_lock)
        {
            // If index is invalid
            if (index < 0 || index >= _elements.Length)
                return false;

            // If element is container
            if (_elements[_selectedIndex] is not INavigableElement)
                return false;

            // If element is already selected
            if(_isElementSelected && _selectedIndex == index)
                return false;

            // If element is already selected try to deselect it and select new one
            if (!TryDeselect())
                return false;

            // Select
            OnSelect?.Invoke((INavigableElement)_elements[_selectedIndex]);
            _isElementSelected = true;
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
    public bool Deselect()
    {
        lock(_lock)
        {
            // If already not selected
            if (!_isElementSelected)
                return false;

            // If selected element is blocking navigation
            if (_elements[_selectedIndex].IsLocked())
                return false;

            // Deselect
            _isElementSelected = false;
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