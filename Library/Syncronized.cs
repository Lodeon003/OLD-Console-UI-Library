using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lodeon.Terminal
{
    internal struct Syncronized<T>
    {
        private T _value;
        private object _lock;

        public Syncronized(T value)
        {
            _value = value;
            _lock = new object();
        }

        public Syncronized() { _value = default!; _lock = new object(); }

        public T Get()
        {
            lock(_lock)
            {
                return _value;
            }
        }

        public void Lock(Action<T> code)
        {
            lock(_lock)
            {
                code.Invoke(_value);
            }
        }
     
        public void Set(T value)
        {
            lock(_lock)
            {
                _value = value;
            }
        }
        //public static implicit operator Syncronized<T>(T value) => new Syncronized<T>(value);
        //public static implicit operator T(Syncronized<T> value) => value;
    }
}
