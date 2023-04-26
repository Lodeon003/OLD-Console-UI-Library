using Lodeon.Terminal.UI.Layout;
using System.Linq.Expressions;
using System.Reflection;

namespace Lodeon.Terminal.UI;

public class ElementCache
{
    private Dictionary<string, Func<Element>> _constructors = new();

    // Absolutely to test
    public ElementCache()
    {
        // Scan every assembly loaded and check for types deriving from 'Element' class that have 'Name' attribute
        IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies().SelectMany((assembly) => assembly.GetTypes()
                                                        .Where((type) => type.IsAssignableFrom(typeof(Element)) && !type.IsAbstract && !type.IsGenericType)
                                                        .Where((type) => type.GetCustomAttribute<NameAttribute>() != null));

        foreach (Type t in types)
        {
            NewExpression exp = Expression.New(t);
            Func<Element> ctor = (Func<Element>)Expression.Lambda(exp, true, null).Compile();

            if(!_constructors.TryAdd(t.GetCustomAttribute<NameAttribute>().Name.ToLower(), ctor))
                throw new Exception("More than one element has the same name");
        }
    }

    /// <summary>
    /// Returns the current element cache or creates a new one if needed<br/>
    /// Assemblies will be scanned the first time this instance is called
    /// </summary>
    public static ElementCache Instance => _instance ??= new ElementCache();
    private static ElementCache? _instance;

    public Element Instantiate(string elementName)
    {
        // convert to lowercase
        elementName = elementName.ToLower();

        if(_constructors.TryGetValue(elementName, out Func<Element>? constructor))
            return constructor();

        throw new ArgumentException($"No element was found in loaded assemblies with name '{elementName}'");
    }
}