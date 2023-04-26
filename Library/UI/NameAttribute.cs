public class NameAttribute : Attribute
{
    public string Name {get; private init;}

    public NameAttribute(string name)
    {
        Name = name;
    }
}