internal class UIElementCache
{
    private Dictionary<string, Function<Element>> _constructors = new();

    public UIElementCache()
    {
        IEnumerable<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies();
        IEnumerable<Type> types = assemblies.Select((assembly) => assembly.GetTypes()
                                                        .Where((type) => type.IsAssignableFrom(typeof(Element) && !type.IsAbstract))
                                                        .Where((type).GetCustomAttribute<NameAttribute>() != null));

        foreach(Type t in types)
        {
            Expression<Func<Element>> exp = Expression.New(t);
            Func<Element> ctor = exp.Compile();

            if(!_constructors.TryAdd(t.GetCustomAttribute<NameAttribute>()[0].Name.ToLower(), ctor))
                throw new Exception("More than one element has the same name");
        }
    }

    public static UIElementCache? Instance => _instance ?? _instance = new LayoutElementCache();
    private UIElementCache _instance;

    public Element? Instantiate(string elementName)
    {
        // convert to lowercase
        elementName = elementName.ToLower();

        if(_constructors.TryGet(elementName, out Element element))
            return element;

        throw new ArgumentException($"No element was found in loaded assemblies with name '{elementName}'");
    }
}