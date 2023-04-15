namespace Lodeon.Terminal.UI;

public abstract class Page
{
    public bool IsMain { get; private set; }

    public void SetMain()
    { IsMain = true; }

    protected abstract void Load();
    protected virtual void OnInitialize() { }
}
