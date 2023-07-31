using BoneLib;
using LabFusion.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Data {
    public class FusionDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> {
        private readonly bool _useDictionary = false;

        // PC
        private Dictionary<TKey, TValue> _internalDictionary;

        // Android
        private List<TKey> _internalKeys;
        private List<TValue> _internalValues;

        public ICollection<TKey> Keys {
            get {
                if (_useDictionary) {
                    return _internalDictionary.Keys;
                }
                else {
                    return _internalKeys;
                }
            }
        }

        public ICollection<TValue> Values {
            get {
                if (_useDictionary) {
                    return _internalDictionary.Values;
                }
                else {
                    return _internalValues;
                }
            }
        }

        public int Count {
            get {
                if (_useDictionary) {
                    return _internalDictionary.Count;
                }
                else {
                    return _internalKeys.Count;
                }
            }
        }

        public bool Remove(TKey key) {
            if (_useDictionary) {
                return _internalDictionary.Remove(key);
            }
            else {
                for (var i = 0; i < _internalKeys.Count; i++) {
                    if (CompareKeys(_internalKeys[i], key)) {
                        _internalKeys.RemoveAt(i);
                        _internalValues.RemoveAt(i);
                        return true;
                    }
                }

                return false;
            }
        }

        public bool CompareKeys(TKey lft, TKey rht) {
            return lft.EqualsIL2CPP(rht);
        }

        public bool CompareValues(TValue lft, TValue rht) {
            return lft.EqualsIL2CPP(rht);
        }

        public void Clear() {
            if (_useDictionary) {
                _internalDictionary.Clear();
            }
            else {
                _internalKeys.Clear();
                _internalValues.Clear();
            }
        }

        public void Add(TKey key, TValue value) {
            if (_useDictionary) {
                _internalDictionary.Add(key, value);
            }
            else {
                // ArgumentNullException
                if (key == null)
                    throw new ArgumentNullException(nameof(key));

                // ArgumentException
                if (ContainsKey(key))
                    throw new ArgumentException("key already exists in the dictionary.");

                // Add
                _internalKeys.Add(key);
                _internalValues.Add(value);
            }
        }

        public bool ContainsValue(TValue value)
        {
            if (_useDictionary) {
                return _internalDictionary.ContainsValue(value);
            }
            else {
                for (var i = 0; i < _internalValues.Count; i++) {
                    if (CompareValues(_internalValues[i], value)) {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool ContainsKey(TKey key) {
            if (_useDictionary) {
                return _internalDictionary.ContainsKey(key);
            }
            else {
                for (var i = 0; i < _internalKeys.Count; i++) {
                    if (CompareKeys(_internalKeys[i], key)) {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool TryGetValue(TKey key, out TValue value) {
            if (_useDictionary) {
                return _internalDictionary.TryGetValue(key, out value);
            }
            else {
                for (var i = 0; i < _internalKeys.Count; i++) {
                    if (CompareKeys(_internalKeys[i], key)) {
                        value = _internalValues[i];
                        return true;
                    }
                }
            }

            value = default;
            return false;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            if (_useDictionary) {
                return _internalDictionary.GetEnumerator();
            }
            else {
                return new FusionDictionaryEnumerator<TKey, TValue>(_internalKeys, _internalValues);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public TValue this[TKey key] {
            get {
                if (_useDictionary) {
                    return _internalDictionary[key];
                }
                else {
                    for (var i = 0; i < _internalKeys.Count; i++) {
                        if (CompareKeys(_internalKeys[i], key)) {
                            return _internalValues[i];
                        }
                    }

                    throw new KeyNotFoundException();
                }
            }
            set {
                if (_useDictionary) {
                    _internalDictionary[key] = value;
                }
                else {
                    for (var i = 0; i < _internalKeys.Count; i++) {
                        if (CompareKeys(_internalKeys[i], key)) {
                            _internalValues[i] = value;
                            return;
                        }
                    }
                    throw new KeyNotFoundException();
                }
            }
        }

        public FusionDictionary() : this(0, null) { }

        public FusionDictionary(int capacity) : this(capacity, null) { }

        public FusionDictionary(IEqualityComparer<TKey> comparer) : this(0, comparer) { }

        public FusionDictionary(int capacity, IEqualityComparer<TKey> comparer) {
            // Dictionaries work normal on PC
            if (!HelperMethods.IsAndroid()) {
                _internalDictionary = new(capacity, comparer);
                _useDictionary = true;
            }
            else {
                _internalKeys = new(capacity);
                _internalValues = new(capacity);
                _useDictionary = false;
            }
        }

        public static implicit operator FusionDictionary<TKey, TValue>(Dictionary<TKey, TValue> dict) {
            if (!HelperMethods.IsAndroid()) {
                return new FusionDictionary<TKey, TValue> {
                    _internalDictionary = dict
                };
            }
            else {
                return new FusionDictionary<TKey, TValue> {
                    _internalKeys = dict.Keys.ToList(),
                    _internalValues = dict.Values.ToList(),
                };
            }
        }

        public static implicit operator Dictionary<TKey, TValue>(FusionDictionary<TKey, TValue> dict) {
            if (dict._useDictionary) {
                return dict._internalDictionary;
            }
            else {
                var dictionary = new Dictionary<TKey, TValue>(dict.Count);
                for (var i = 0; i < dict.Count; i++) {
                    dictionary.Add(dict.Keys.ElementAt(i), dict.Values.ElementAt(i));
                }
                return dictionary;
            }
        }
    }

    public class FusionDictionaryEnumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        public List<TKey> keys;
        public List<TValue> values;

        int position = -1;

        public FusionDictionaryEnumerator(List<TKey> keys, List<TValue> values) {
            this.keys = keys;
            this.values = values;
        }

        public bool MoveNext() {
            position++;
            return position < keys.Count;
        }

        public void Reset() {
            position = -1;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        object IEnumerator.Current {
            get {
                return Current;
            }
        }

        public KeyValuePair<TKey, TValue> Current
        {
            get
            {
                try
                {
                    return new KeyValuePair<TKey, TValue>(keys[position], values[position]);
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}
