using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityExtension.Runtime.extension.Scripts.Runtime.Components;
using UnitySceneBase.Runtime.scene_system.scene_base.Scripts.Runtime.Types;

namespace UnitySceneBase.Runtime.scene_system.scene_base.Scripts.Runtime.Components
{
    public sealed class SceneParameterSystem : SearchingSingletonBehavior<SceneParameterSystem>
    {
        internal static void UpdateParameterData(ParameterData parameterData, bool overwrite = true)
        {
            if (parameterData == null)
                return;
            
            var data = Singleton._data;
            if (!data.ContainsKey(parameterData.GetType()))
            {
                data.Add(parameterData.GetType(), parameterData);
            }
            else
            {
                data[parameterData.GetType()].Update(parameterData, overwrite);
            }
        }

        public static bool HasData<T>() where T : ParameterData
        {
            return Singleton._data.ContainsKey(typeof(T));
        }

        public static T GetData<T>(ScriptableObject[] scriptableObjects) where T : ParameterData
        {
            var data = Singleton._data;
            if (!data.ContainsKey(typeof(T)))
            {
                var parameterData = (T) typeof(T).GetConstructor(Type.EmptyTypes).Invoke(Array.Empty<object>());
                var attribute = typeof(T).GetCustomAttribute<ParameterInitialDataTypeAttribute>();
                if (attribute == null)
                {
                    parameterData.InitializeData(null);
                }
                else
                {
                    var scriptableObject = scriptableObjects.FirstOrDefault(x => x.GetType() == attribute.Type);
                    parameterData.InitializeData(scriptableObject);
                }

                return parameterData;
            }

            return (T)data[typeof(T)];
        }

        private readonly IDictionary<Type, ParameterData> _data = new Dictionary<Type, ParameterData>();
    }
}