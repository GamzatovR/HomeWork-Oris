using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniTemplateEngine
{
    class ScopedModel : IDictionary<string, object>
    {
        private readonly Dictionary<string, object> _local;
        private readonly object _parentModel;

        public ScopedModel(object parentModel)
        {
            _local = new Dictionary<string, object>();
            _parentModel = parentModel;
        }

        public void SetLocal(string name, object value) => _local[name] = value;

        public bool TryGetLocal(string name, out object value) => _local.TryGetValue(name, out value);

        public bool TryGetValue(string key, out object value)
        {
            if (_local.TryGetValue(key, out value)) return true;

            value = HtmlTemplateRenderer.GetPropertyValue(_parentModel, key);
            return value != null;
        }

        public object this[string key] { get => (_local.ContainsKey(key) ? _local[key] : HtmlTemplateRenderer.GetPropertyValue(_parentModel, key)); set => _local[key] = value; }
        public ICollection<string> Keys => _local.Keys;
        public ICollection<object> Values => _local.Values;
        public int Count => _local.Count;
        public bool IsReadOnly => false;
        public void Add(string key, object value) => _local.Add(key, value);
        public void Add(KeyValuePair<string, object> item) => _local.Add(item.Key, item.Value);
        public void Clear() => _local.Clear();
        public bool Contains(KeyValuePair<string, object> item) => _local.ContainsKey(item.Key) && Equals(_local[item.Key], item.Value);
        public bool ContainsKey(string key) => _local.ContainsKey(key) || HtmlTemplateRenderer.GetPropertyValue(_parentModel, key) != null;
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => throw new NotImplementedException();
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _local.GetEnumerator();
        public bool Remove(string key) => _local.Remove(key);
        public bool Remove(KeyValuePair<string, object> item) => _local.Remove(item.Key);
        IEnumerator IEnumerable.GetEnumerator() => _local.GetEnumerator();
    }
}
