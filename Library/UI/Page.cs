namespace Lodeon.Terminal.UI;

public abstract class Page
{
    protected Driver Out { get { if (_driver is null) throw new ArgumentNullException(this.GetType().FullName, "Element was not initialized"); return _driver; } }
    private Driver? _driver;
    public bool IsMain { get; private set; }

    /// <summary>
    /// Can't be put in constructor because a generic type deriving from <see cref="Page"/> can't specify a constructor
    /// with parameters
    /// </summary>
    /// <param name="isMain"></param>
    public void Initialize(Driver driver, bool isMain)
    {
        _driver = driver;
        IsMain = isMain;
    }

    protected abstract void Load();
    
    // These two: to implement
    //protected abstract void Main();
    //protected abstract void OnExit();
    
    
    //protected virtual void OnInitialize() { }
}
