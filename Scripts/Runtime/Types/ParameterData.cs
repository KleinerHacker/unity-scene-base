using System;
using System.Collections.Generic;

namespace UnitySceneBase.Runtime.scene_system.scene_base.Scripts.Runtime.Types
{
    public sealed class ParameterData
    {
        private readonly IDictionary<string, object> _data = new Dictionary<string, object>();

        public void Add<T>(string key, T value, bool overwrite = true)
        {
            if (!overwrite && Exists(key))
                throw new InvalidOperationException("The key " + key + " is already contains in " + nameof(ParameterData));

            if (Exists(key))
            {
                _data[key] = value;
            }
            else
            {
                _data.Add(key, value);
            }
        }

        public bool Exists(string key) => _data.ContainsKey(key);

        public void Remove(string key, bool mustExists = false)
        {
            if (mustExists && !Exists(key))
                throw new InvalidOperationException("The key " + key + " is not contained in " + nameof(ParameterData));
            
            if (!Exists(key))
                return;

            _data.Remove(key);
        }

        public T Get<T>(string key, bool mustExists = false)
        {
            if (mustExists && !Exists(key))
                throw new InvalidOperationException("The key " + key + " is not contained in " + nameof(ParameterData));

            if (!Exists(key))
                return default;

            return (T)_data[key];
        }

        internal void Update(ParameterData parameterData, bool overwrite = true)
        {
            foreach (var item in parameterData._data)
            {
                Add(item.Key, item.Value, overwrite);
            }
        }
    }
}