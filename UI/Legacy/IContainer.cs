namespace Lodeon.Terminal.UI;

public interface IContainer<T>
{
    void AddItem(T element);
    void RemoveItem(T element);
    ReadOnlySpan<T> GetItems();
}