using System.Collections.Generic;

namespace test.Parsers.Base
{
    public sealed class ParseResult<T>
    {
        private readonly object _lock = new object();
        private readonly List<T> _valid = new List<T>();
        private readonly List<string> _invalid = new List<string>();

        public IReadOnlyList<T> Valid { get { return _valid; } }
        public IReadOnlyList<string> InvalidHighlighted { get { return _invalid; } }

        public bool HasError { get { return _invalid.Count > 0; } }

        public void AddValid(T item)
        {
            lock (_lock)
            {
                _valid.Add(item);
            }
        }

        public void AddError(string error)
        {
            lock (_lock)
            {
                _invalid.Add(error);
            }
        }
    }
}
