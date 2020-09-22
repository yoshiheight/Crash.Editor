using System.Collections.Generic;

namespace Crash.Core.Collections
{
    public sealed class PooledList<TElem>
        where TElem : class, new()
    {
        private int _count;

        private readonly List<TElem> _list = new List<TElem>();

        public TElem Add()
        {
            _count++;
            if (_list.Count < _count)
            {
                _list.Add(new TElem());
            }
            return _list[_count - 1];
        }

        public TElem? LastOrNull()
        {
            return (_count > 0) ? _list[_count - 1] : null;
        }

        public IEnumerable<TElem> GetElements()
        {
            for (var i = 0; i < _count; i++)
            {
                yield return _list[i];
            }
        }

        public IEnumerable<TElem> GetElementsAndClear()
        {
            try
            {
                foreach (var elem in GetElements())
                {
                    yield return elem;
                }
            }
            finally
            {
                Clear();
            }
        }

        public void Clear()
        {
            _count = 0;
            //_list.TrimExcess();
        }
    }
}
