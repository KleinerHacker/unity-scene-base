using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityCommonEx.Runtime.common_ex.Scripts.Runtime.Utils;
using UnityEngine;
using UnitySceneBase.Runtime.scene_system.scene_base.Scripts.Runtime.Types;

namespace UnitySceneBase.Runtime.scene_system.scene_base.Scripts.Runtime.Utils
{
    internal static class SceneEventUtils
    {
        public static bool HasSceneEvents => _sceneEvents != null && _sceneEvents.Count > 0 && _sceneEvents.All(x => x.Value.Count > 0);
        public static bool HasBlendingEvents => _blendEvents != null && _blendEvents.Count > 0 && _blendEvents.All(x => x.Value.Count > 0);
        
        private static readonly IDictionary<RuntimeOnSwitchSceneType, List<MethodInfo>> _sceneEvents;
        private static readonly IDictionary<RuntimeOnBlendSceneType, List<MethodInfo>> _blendEvents;

        static SceneEventUtils()
        {
            Debug.Log("Search for scene system callbacks (blending)");
            _blendEvents = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .SelectMany(x => x.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .Where(x => x.GetCustomAttribute<RuntimeOnBlendSceneAttribute>() != null)
                .GroupBy(x => x.GetCustomAttribute<RuntimeOnBlendSceneAttribute>().Type)
                .ToDictionary(x => x.Key, x => x.ToList());
            
            Debug.Log("Search for scene system callbacks (scenes)");
            _sceneEvents = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .SelectMany(x => x.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .Where(x => x.GetCustomAttribute<RuntimeOnSwitchSceneAttribute>() != null)
                .GroupBy(x => x.GetCustomAttribute<RuntimeOnSwitchSceneAttribute>().Type)
                .ToDictionary(x => x.Key, x => x.ToList());
        }

        public static int GetSceneEventCount(RuntimeOnSwitchSceneType type) => 
            _sceneEvents == null ? 0 : !_sceneEvents.ContainsKey(type) ? 0 : _sceneEvents[type].Count;

        public static void RaiseSceneEvent(RuntimeOnSwitchSceneType type, RuntimeOnSwitchSceneArgs args, Action<string[]> visitor)
        {
            if (_sceneEvents == null || !_sceneEvents.ContainsKey(type))
                return;
            
            foreach (var methodInfo in _sceneEvents[type])
            {
                methodInfo.Invoke(null, new object[] { args });
                visitor.Invoke(args.AdditionalScenes);
            }
        }

        public static int GetBlendEventCount(RuntimeOnBlendSceneType type) =>
            _blendEvents == null ? 0 : !_blendEvents.ContainsKey(type) ? 0 : _blendEvents[type].Count;
        
        public static void RaiseBlendEvent(RuntimeOnBlendSceneType type, RuntimeOnBlendSceneArgs args)
        {
            if (_blendEvents == null || !_blendEvents.ContainsKey(type))
                return;
            
            foreach (var methodInfo in _blendEvents[type])
            {
                methodInfo.Invoke(null, new object[] { args });
            }
        }
    }
}