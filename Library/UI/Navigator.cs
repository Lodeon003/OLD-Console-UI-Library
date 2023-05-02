namespace Lodeon.Terminal.UI
{
    public abstract class Navigator
    {
        public enum ErrorCode
        {
            NotFoundByKey,
            NotFoundByPredicate,
            ElementAlreadyPesent
        }
    }

    public class Navigator<T1, T2> : Navigator where T1 : notnull where T2 : notnull
    {
        private Dictionary<T1, T2> _elements;
        private object _lock = new object();

        public Navigator(Dictionary<T1, T2> pages)
            => this._elements = pages;

        public delegate void NavigatorDel(T2 value);
        public delegate void NavigatorFailDel(ErrorCode error);
        public delegate void NavigatorExitDel();
        public event NavigatorDel? OnNavigate;
        public event NavigatorFailDel? OnNavigateFail;
        public event NavigatorExitDel? OnExit;

        public bool Add(T1 key, T2 value)
        {
            lock(_lock)
            {
                 return _elements.TryAdd(key, value);
            }
        }

        public bool Remove(T1 key)
        {
            lock(_lock)
            {
                return _elements.Remove(key);
            }
        }

        public voif Iterate(Action<T2> code)
        {
           lock(_lock)
           {
               foreach(var pair in _elements)
                  code.Invoke(pair.Value);
           }
        }

        public void Navigate(Func<T2, bool> predicate)
        {
            lock(_lock)
            {
                foreach(T2 element in _elements.Values)
                {
                    if (!predicate.Invoke(element))
                        continue;

                    Task.Run(() => OnNavigate?.Invoke(element));
                    break;
                }

                Task.Run(() => OnNavigateFail?.Invoke(ErrorCode.NotFoundByPredicate));
            }
        }

        public void Navigate(T1 key)
        {
            lock (_lock)
            {
                if(_elements.TryGetValue(key, out T2? value))
                    Task.Run(() => OnNavigate?.Invoke(value));
                else
                    Task.Run(() => OnNavigateFail?.Invoke(ErrorCode.NotFoundByKey));
            }
        }

        public void Navigate(T2 value)
        {
            Task.Run(() => OnNavigate?.Invoke(value));
        }

        public void Exit()
        {
            Task.Run(() => OnExit?.Invoke());
        }
    }
}
