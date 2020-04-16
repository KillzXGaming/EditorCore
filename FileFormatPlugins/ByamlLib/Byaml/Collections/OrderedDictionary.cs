using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ByamlExt.Byaml
{
    /// <summary>
    /// Represents a generic dictionary which preserves the order in which items were added, inserted, or reordered. The
    /// items can be accessed either by index or key.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values associated with the keys.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(OrderedDictionary<,>.DebugView))]
    public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        private List<KeyValuePair<TKey, TValue>> _keyValuePairs;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}"/> class.
        /// </summary>
        public OrderedDictionary()
        {
            _keyValuePairs = new List<KeyValuePair<TKey, TValue>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedDictionary{TKey, TValue}"/> class with the given
        /// preininitalized <paramref name="capacity"/>.
        /// </summary>
        /// <param name="capacity">The number of items to reserve initially.</param>
        public OrderedDictionary(int capacity)
        {
            _keyValuePairs = new List<KeyValuePair<TKey, TValue>>(capacity);
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets a collection containing the keys in the dictionary. 
        /// </summary>
        public ICollection<TKey> Keys => _keyValuePairs.Select(x => x.Key).ToList();

        /// <summary>
        /// Gets a collection containing the values in the dictionary.
        /// </summary>
        public ICollection<TValue> Values => _keyValuePairs.Select(x => x.Value).ToList();

        /// <summary>
        /// Gets the number of key/value pairs contained in the dictionary.
        /// </summary>
        public int Count => _keyValuePairs.Count;

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => ((IList)_keyValuePairs).IsReadOnly;

        // ---- OPERATORS ----------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the value at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index at which to set the value.</param>
        /// <returns>The value at the given index.</returns>
        public TValue this[int index]
        {
            get => _keyValuePairs[index].Value;
            set => _keyValuePairs[index] = new KeyValuePair<TKey, TValue>(_keyValuePairs[index].Key, value);
        }

        /// <summary>
        /// Gets or sets the value associated with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the key.</returns>
        public TValue this[TKey key]
        {
            get
            {
                foreach (KeyValuePair<TKey, TValue> kvp in _keyValuePairs)
                {
                    if (EqualityComparer<TKey>.Default.Equals(kvp.Key, key))
                        return kvp.Value;
                }
                throw new KeyNotFoundException($"Key \"{key}\" was not found in the dictionary.");
            }
            set
            {
                for (int i = 0; i < _keyValuePairs.Count; i++)
                {
                    KeyValuePair<TKey, TValue> kvp = _keyValuePairs[i];
                    if (EqualityComparer<TKey>.Default.Equals(kvp.Key, key))
                    {
                        _keyValuePairs[i] = new KeyValuePair<TKey, TValue>(kvp.Key, value);
                        return;
                    }
                }
                throw new KeyNotFoundException($"Key \"{key}\" was not found in the dictionary.");
            }
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Adds an entry with the specified key and value into the <see cref="OrderedDictionary{TKey, TValue}"/>
        /// collection with the lowest available index.
        /// </summary>
        /// <param name="key">The key of the entry to add.</param>
        /// <param name="value">The value of the entry to add. This value can be <see langword="null"/>.</param>
        public void Add(TKey key, TValue value)
        {
            if (TryGetValue(key, out TValue _))
                throw new ArgumentException($"Key \"{key}\" already exists in the dictionary.", nameof(key));

            _keyValuePairs.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <summary>
        /// Removes all elements from the <see cref="OrderedDictionary{TKey, TValue}"/> collection.
        /// </summary>
        public void Clear() => _keyValuePairs.Clear();

        /// <summary>
        /// Determines whether the <see cref="OrderedDictionary{TKey, TValue}"/> collection contains a specific
        /// <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="OrderedDictionary{TKey, TValue}"/> collection.</param>
        /// <returns>
        /// <see langword="true"/> if the <see cref="OrderedDictionary{TKey, TValue}"/> collection contains an
        /// element with the specified key; otherwise, <see langword="false"/>.
        /// </returns>
        public bool ContainsKey(TKey key) => TryGetValue(key, out TValue _);

        /// <summary>
        /// Copies the <see cref="OrderedDictionary{TKey, TValue}"/> elements to a one-dimensional <see cref="Array"/>
        /// object at the specified index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> object that is the destination of the
        /// <see cref="KeyValuePair{TKey, TValue}"/> objects copied from the
        /// <see cref="OrderedDictionary{TKey, TValue}"/> collection. The <see cref="Array"/> must have zero-based
        /// indexing.
        /// </param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public void CopyTo(Array array, int index) => _keyValuePairs.CopyTo((KeyValuePair<TKey, TValue>[])array, index);

        /// <summary>
        /// Returns an <see cref="IDictionaryEnumerator"/> object that iterates through the
        /// <see cref="OrderedDictionary{TKey, TValue}"/> collection.
        /// </summary>
        /// <returns>An <see cref="IDictionaryEnumerator"/> object for the <see cref="OrderedDictionary{TKey, TValue}"/>
        /// collection.</returns>
        public IEnumerator GetEnumerator() => _keyValuePairs.GetEnumerator();

        /// <summary>
        /// Inserts a new entry into the <see cref="OrderedDictionary{TKey, TValue}"/> collection with the specified
        /// <paramref name="key"/> and <paramref name="value"/> at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The zero-based index at which the element should be inserted.</param>
        /// <param name="key">The key of the entry to add.</param>
        /// <param name="value">The value of the entry to add. The value can be <see langword="null"/>.</param>
        public void Insert(int index, TKey key, TValue value)
        {
            if (TryGetValue(key, out TValue _))
                throw new ArgumentException($"Key \"{key}\" already exists in the dictionary.", nameof(key));

            _keyValuePairs.Insert(index, new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <summary>
        /// Removes the entry with the specified key from the <see cref="OrderedDictionary{TKey, TValue}"/> collection.
        /// </summary>
        /// <param name="key">The key of the entry to remove.</param>
        /// <returns><see langword="true"/> if an entry was actually removed.</returns>
        public bool Remove(TKey key)
        {
            for (int i = 0; i < _keyValuePairs.Count; i++)
            {
                if (EqualityComparer<TKey>.Default.Equals(_keyValuePairs[i].Key, key))
                {
                    _keyValuePairs.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes the entry at the specified index from the <see cref="OrderedDictionary{TKey, TValue}"/> collection.
        /// </summary>
        /// <param name="index">The zero-based index of the entry to remove.</param>
        public void RemoveAt(int index) => _keyValuePairs.RemoveAt(index);

        /// <summary>
        /// Gets the value associated with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        /// When this method returns, contains the value associated with the specified key, if the
        /// key is found; otherwise, the default value for the type of the value parameter. This parameter is passed
        /// uninitialized.
        /// </param>
        /// <returns><see langword="true"/> if the <see cref="OrderedDictionary{TKey, TValue}"/> contains an element
        /// with the specified key; otherwise, <see langword="false"/>.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            foreach (KeyValuePair<TKey, TValue> kvp in _keyValuePairs)
            {
                if (EqualityComparer<TKey>.Default.Equals(kvp.Key, key))
                {
                    value = kvp.Value;
                    return true;
                }
            }
            value = default;
            return false;
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => _keyValuePairs.Contains(item);
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => _keyValuePairs.CopyTo(array, arrayIndex);
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => _keyValuePairs.GetEnumerator();
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => _keyValuePairs.Remove(item);

        // ---- CLASSES, STRUCTS & ENUMS -------------------------------------------------------------------------------

        internal sealed class DebugView
        {
            private OrderedDictionary<TKey, TValue> _instance;

            public DebugView(OrderedDictionary<TKey, TValue> instance)
            {
                _instance = instance ?? throw new ArgumentNullException(nameof(instance));
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public KeyValuePair<TKey, TValue>[] Items => _instance._keyValuePairs.ToArray();
        }
    }
}
