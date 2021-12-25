using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySceneBase.Runtime.scene_system.scene_base.Scripts.Runtime.Types
{
    public class ParameterData
    {
        private readonly IDictionary<string, object> _data = new Dictionary<string, object>();

        public virtual void InitializeData(ScriptableObject initData)
        {
            //Empty
        }

        public void Add<T>(string key, T value, bool overwrite = true)
        {
#if SCENE_VERBOSE
            Debug.Log("[SceneSystem] Try to add " + key + " to " + GetType().FullName);
#endif
            
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
#if SCENE_VERBOSE
            Debug.Log("[SceneSystem] Try to remove " + key + " in " + GetType().FullName);
#endif
            
            if (mustExists && !Exists(key))
                throw new InvalidOperationException("The key " + key + " is not contained in " + nameof(ParameterData));

            if (!Exists(key))
                return;

            _data.Remove(key);
        }

        public T Get<T>(string key, bool mustExists = false)
        {
#if SCENE_VERBOSE
            Debug.Log("[SceneSystem] Try to get " + key + " in " + GetType().FullName);
#endif
            
            if (mustExists && !Exists(key))
                throw new InvalidOperationException("The key " + key + " is not contained in " + nameof(ParameterData));

            if (!Exists(key))
                return default;

            return (T)_data[key];
        }

        internal void Update(ParameterData parameterData, bool overwrite = true)
        {
            if (parameterData == null)
                return;

#if SCENE_VERBOSE
            Debug.Log("[SceneSystem] Update parameter data in " + GetType().FullName);
#endif
            foreach (var item in parameterData._data)
            {
                Add(item.Key, item.Value, overwrite);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ParameterInitialDataTypeAttribute : Attribute
    {
        public Type Type { get; }

        public ParameterInitialDataTypeAttribute(Type type)
        {
            Type = type;
        }
    }
}