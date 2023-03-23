using System.Collections.ObjectModel;

namespace CascadeEventFramework
{
    public class ObservableDictionary<TKey, TValue> : ObservableCollection<KeyValuePair<TKey, TValue>>
    {
        public ObservableDictionary() : base() { }

        public ObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
            : base(collection) { }

        public TValue this[TKey key]
        {
            get
            {
                var item = this.FirstOrDefault(i => i.Key.Equals(key));
                if (item.Equals(default(KeyValuePair<TKey, TValue>)))
                {
                    throw new KeyNotFoundException();
                }
                return item.Value;
            }
            set
            {
                var item = this.FirstOrDefault(i => i.Key.Equals(key));
                if (!item.Equals(default(KeyValuePair<TKey, TValue>)))
                {
                    this.Remove(item);
                }
                this.Add(new KeyValuePair<TKey, TValue>(key, value));
            }
        }

        public void Add(TKey key, TValue value)
        {
            this.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public bool ContainsKey(TKey key)
        {
            return this.Any(i => i.Key.Equals(key));
        }

        public bool Remove(TKey key)
        {
            var item = this.FirstOrDefault(i => i.Key.Equals(key));
            if (!item.Equals(default(KeyValuePair<TKey, TValue>)))
            {
                return this.Remove(item);
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var item = this.FirstOrDefault(i => i.Key.Equals(key));
            if (!item.Equals(default(KeyValuePair<TKey, TValue>)))
            {
                value = item.Value;
                return true;
            }
            value = default(TValue);
            return false;
        }
    }

}
