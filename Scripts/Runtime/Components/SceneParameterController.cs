#if PCSOFT_SCENE || PCSOFT_WORLD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityBase.Runtime.@base.Scripts.Runtime.Components.Singleton;
using UnityBase.Runtime.@base.Scripts.Runtime.Components.Singleton.Attributes;
using UnityEngine;
using UnitySceneBase.Runtime.scene_system.scene_base.Scripts.Runtime.Types;

namespace UnitySceneBase.Runtime.scene_system.scene_base.Scripts.Runtime.Components
{
    [Singleton(Scope = SingletonScope.Application, Instance = SingletonInstance.RequiresNewInstance, CreationTime = SingletonCreationTime.Loading, ObjectName = "Scene System")]
    public sealed class SceneParameterController : SingletonBehavior<SceneParameterController>
    {
        internal static void UpdateParameterData(ParameterData parameterData, ScriptableObject[] scriptableObjects, bool overwrite = true)
        {
            if (parameterData == null)
                return;

#if PCSOFT_SCENE_VERBOSE
            Debug.Log("[SceneSystem] Update parameter data");
#endif
            var parameter = GetData(parameterData.GetType(), scriptableObjects);
            parameter.Update(parameterData, overwrite);
        }

        public static bool HasData<T>() where T : ParameterData
        {
            return Singleton._data.ContainsKey(typeof(T));
        }

        public static T GetData<T>(ScriptableObject[] scriptableObjects) where T : ParameterData
        {
            return (T)GetData(typeof(T), scriptableObjects);
        }

        public static ParameterData GetData(Type type, ScriptableObject[] scriptableObjects)
        {
            if (!typeof(ParameterData).IsAssignableFrom(type))
                throw new ArgumentException("Type " + type.FullName + " is not a " + nameof(ParameterData) + " type");
            
#if PCSOFT_SCENE_VERBOSE
            Debug.Log("[SceneSystem] Try to get data of type " + type.FullName);
#endif

            var data = Singleton._data;
            if (!data.ContainsKey(type))
            {
#if PCSOFT_SCENE_VERBOSE
                Debug.Log("[SceneSystem] ... data not found, create new for type " + type.FullName);
                Debug.Log("[SceneSystem] > " + string.Join(',', data.Keys));
#endif
                
                var parameterData = (ParameterData)type.GetConstructor(Type.EmptyTypes).Invoke(Array.Empty<object>());
                var attribute = type.GetCustomAttribute<ParameterInitialDataTypeAttribute>();
                if (attribute == null)
                {
#if PCSOFT_SCENE_VERBOSE
                    Debug.Log("[SceneSystem] No initial data found for type " + type.FullName);
#endif
                    parameterData.InitializeData(null);
                }
                else
                {
#if PCSOFT_SCENE_VERBOSE
                    Debug.Log("[SceneSystem] Initialize data for type " + type.FullName);
#endif
                    var scriptableObject = scriptableObjects.FirstOrDefault(x => x.GetType() == attribute.Type);
                    parameterData.InitializeData(scriptableObject);
                }

                data.Add(type, parameterData);
                return parameterData;
            }

            return data[type];
        }

        private readonly IDictionary<Type, ParameterData> _data = new Dictionary<Type, ParameterData>();
    }
}
#endif