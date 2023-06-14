using System.Collections;
using IL.RankedCache.CacheProvider;

namespace IL.RankedCache.CacheAccessCounter
{
    internal class DistributedCacheAccessCounter<TCacheCounterOrder> : ICacheAccessCounter<TCacheCounterOrder> where TCacheCounterOrder : struct
    {
        private const string CounterSuffix = "_count";
        private readonly ICacheProvider _cacheProvider;
        private readonly HashSet<string> _counterNames = new();
        public int Count => _counterNames.Count;
        public bool IsReadOnly => false;

        public DistributedCacheAccessCounter(ICacheProvider cacheProvider)
        {
            _cacheProvider = cacheProvider;
        }

        public IEnumerator<KeyValuePair<string, TCacheCounterOrder>> GetEnumerator()
        {
            return _counterNames
                .Select(x => new KeyValuePair<string, TCacheCounterOrder>(x, _cacheProvider.Get<TCacheCounterOrder>(x).Result))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, TCacheCounterOrder> item)
        {
            var composedKey = KeyWithSuffix(item.Key);
            _counterNames.Add(composedKey);
            _cacheProvider.Add(composedKey, item.Value);
        }

        public void Clear()
        {
            foreach (var counterName in _counterNames)
            {
                _cacheProvider.Delete(counterName).Wait();
            }
            _counterNames.Clear();
        }

        public bool Contains(KeyValuePair<string, TCacheCounterOrder> item)
        {
            return _counterNames.Contains(KeyWithSuffix(item.Key));
        }

        public void CopyTo(KeyValuePair<string, TCacheCounterOrder>[] array, int arrayIndex)
        {
            return;
        }

        public bool Remove(KeyValuePair<string, TCacheCounterOrder> item)
        {
            var composedKey = KeyWithSuffix(item.Key);
            _cacheProvider.Delete(composedKey).Wait();
            return _counterNames.Remove(composedKey);
        }

        public void Add(string key, TCacheCounterOrder value)
        {
            var composedKey = KeyWithSuffix(key);
            _counterNames.Add(composedKey);
            _cacheProvider.Add(composedKey, value);
        }

        public bool ContainsKey(string key)
        {
            return _counterNames.Contains(KeyWithSuffix(key));
        }

        public bool Remove(string key)
        {
            var composedKey = KeyWithSuffix(key);
            _cacheProvider.Delete(composedKey).Wait();
            return _counterNames.Remove(composedKey);
        }

        public bool TryGetValue(string key, out TCacheCounterOrder value)
        {
            var composedKey = KeyWithSuffix(key);
            value = _cacheProvider.Get<TCacheCounterOrder>(composedKey).Result;
            return _counterNames.Contains(composedKey);
        }

        public TCacheCounterOrder this[string key]
        {
            get => _cacheProvider.Get<TCacheCounterOrder>(KeyWithSuffix(key)).Result;
            set
            {
                var composedKey = KeyWithSuffix(key);
                if (_cacheProvider.HasKey(composedKey))
                {
                    _cacheProvider.Delete(composedKey).Wait();
                }
                _cacheProvider.Add(composedKey, value).Wait();
                _counterNames.Add(composedKey);
            }
        }

        public ICollection<string> Keys => _counterNames;
        public ICollection<TCacheCounterOrder> Values => _counterNames.Select(x => _cacheProvider.Get<TCacheCounterOrder>(x).Result).ToList();

        private static string KeyWithSuffix(string key)
        {
            return key + CounterSuffix;
        }
    }
}