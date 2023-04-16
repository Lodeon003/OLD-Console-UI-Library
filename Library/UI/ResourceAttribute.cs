namespace Lodeon.Terminal.UI;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class ResourceAttribute : Attribute
{
    public string Path { get; private init; }
    public string Name { get; private init; }

    public ResourceAttribute(string name, string path)
    {
        this.Name = name;
        this.Path = path;
    }
}
