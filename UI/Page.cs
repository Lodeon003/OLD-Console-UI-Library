namespace Lodeon.Terminal.UI;

public abstract class Page
{
    internal Page(Driver output)
    {
        Output = output;
    }

    protected Driver Output { get; private set; }
    public bool IsMain { get; private set; }

    /// <summary>
    /// Can't be put in constructor because a generic type deriving from <see cref="Page"/> can't specify a constructor
    /// with parameters
    /// </summary>
    /// <param name="isMain"></param>
    public void Initialize(Driver driver, bool isMain)
    {
        Output = driver;
        IsMain = isMain;
    }

    protected abstract void Load();
    protected virtual void OnInitialize() { }
}
