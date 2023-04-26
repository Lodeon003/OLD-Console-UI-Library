public class NameAttribute
{
    public string Name {get; private init;}

    public NameAttribute(string name)
    {
        Name = name;
    }
}