using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityBlending.Runtime.scene_system.blending.Scripts.Runtime.Components;
using UnityCommonEx.Runtime.common_ex.Scripts.Runtime.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityExtension.Runtime.extension.Scripts.Runtime.Components;
using UnityExtension.Runtime.extension.Scripts.Runtime.Utils.Extensions;
using UnitySceneBase.Runtime.scene_system.scene_base.Scripts.Runtime.Assets;
using UnitySceneBase.Runtime.scene_system.scene_base.Scripts.Runtime.Types;

namespace UnitySceneBase.Runtime.scene_system.scene_base.Scripts.Runtime.Components
{
    public abstract class SceneSystemBase<T,TI> : SearchingSingletonBehavior<T> where T : SceneSystemBase<T,TI> where TI : SceneItemBase
    {
        #region Static Area

        protected static void LoadSceneSystemBasics(GameObject blendingSystem, GameObjectItem[] items, bool createES, Action<EventSystem> updateES, Action<InputSystemUIInputModule> updateIM)
        {
            Debug.Log("Loading scene system basics");

            if (blendingSystem != null)
            {
                var goBlendingSystem = Instantiate(blendingSystem);
                DontDestroyOnLoad(goBlendingSystem);
            }

            var goParameterSystem = new GameObject("Scene Parameter System");
            goParameterSystem.AddComponent<SceneParameterSystem>();
            DontDestroyOnLoad(goParameterSystem);

            if (createES)
            {
                var goEventSystem = new GameObject("Event System");
                var eventSystem = goEventSystem.AddComponent<EventSystem>();
                updateES(eventSystem);
                var inputModule = goEventSystem.AddComponent<InputSystemUIInputModule>();
                updateIM(inputModule);
                DontDestroyOnLoad(goEventSystem);
            }

            if (items != null && items.Length > 0)
            {
                var parentGo = new GameObject("Additional Game Objects");
                DontDestroyOnLoad(parentGo);
                foreach (var item in items)
                {
                    if (item.Prefab == null)
                    {
                        var go = new GameObject(string.IsNullOrWhiteSpace(item.ObjectName) ? "<game object>" : item.ObjectName);
                        go.transform.SetParent(parentGo.transform);
                        DontDestroyOnLoad(go);
                    }
                    else
                    {
                        var go = Instantiate(item.Prefab, parentGo.transform, true);
                        go.name = item.ObjectName;
                        DontDestroyOnLoad(go);
                    }
                }
            }
        }

        #endregion
        
        #region Properties

        protected abstract bool UseBlendCallbacks { get; }
        protected abstract bool UseSwitchCallbacks { get; }
        protected abstract SceneBlendState StartupBlendState { get; }
        
        public string CurrentState { get; private set; }

        #endregion
        
        private IDictionary<RuntimeOnBlendSceneType, List<MethodInfo>> _blendCallbacks;
        private IDictionary<RuntimeOnSwitchSceneType, List<MethodInfo>> _switchCallbacks;
        
        protected BlendingSystem _blending;

        #region Builtin Methods

        protected virtual void Awake()
        {
            if (UseBlendCallbacks)
            {
                Debug.Log("Search for scene system callbacks (blending)");
                _blendCallbacks = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(x => x.GetTypes())
                    .SelectMany(x => x.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    .Where(x => x.GetCustomAttribute<RuntimeOnBlendSceneAttribute>() != null)
                    .GroupBy(x => x.GetCustomAttribute<RuntimeOnBlendSceneAttribute>().Type)
                    .ToDictionary(x => x.Key, x => x.ToList());
            }

            if (UseSwitchCallbacks)
            {
                Debug.Log("Search for scene system callbacks (switch)");
                _switchCallbacks = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(x => x.GetTypes())
                    .SelectMany(x => x.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    .Where(x => x.GetCustomAttribute<RuntimeOnSwitchSceneAttribute>() != null)
                    .GroupBy(x => x.GetCustomAttribute<RuntimeOnSwitchSceneAttribute>().Type)
                    .ToDictionary(x => x.Key, x => x.ToList());
            }
            
            _blending = FindObjectOfType<BlendingSystem>();
            if (_blending == null)
                throw new InvalidOperationException("Unable to find " + nameof(BlendingSystem));
            
            switch (StartupBlendState)
            {
                case SceneBlendState.Shown:
                    Debug.Log("Start with shown blend");
                    _blending.ShowBlendImmediately();
                    break;
                case SceneBlendState.Hidden:
                    Debug.Log("Start with hidden blend");
                    _blending.HideBlendImmediately();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion
        
        public void Load(string identifier, ParameterData parameterData = null, bool overwrite = true)
        {
            Load(identifier, false, parameterData, overwrite);
        }

        public void Load(string identifier, bool doNotUnload, ParameterData parameterData = null, bool overwrite = true)
        {
            Load(identifier, doNotUnload, null, parameterData, overwrite);
        }

        public void Load(string identifier, Action onFinished, ParameterData parameterData = null, bool overwrite = true)
        {
            Load(identifier, false, onFinished, parameterData, overwrite);
        }

        public void Load(string identifier, bool doNotUnload, Action onFinished, ParameterData parameterData = null, bool overwrite = true)
        {
            var sceneItem = FindSceneItem(identifier);
            if (sceneItem == null)
                throw new InvalidOperationException("Unable to find scene with identifier " + identifier);

            if (!IsAllowNullParameterData(identifier) && parameterData == null)
                throw new InvalidOperationException("Parameter data with NULL value not allowed for " + identifier);
            var parameterDataType = GetAllowedParameterDataType(identifier);
            if (parameterData != null && parameterData.GetType().FullName != parameterDataType)
                throw new InvalidOperationException("Parameter data must of type " + parameterDataType + " for " + identifier);

            SceneParameterSystem.UpdateParameterData(parameterData, overwrite);
            Load(sceneItem, onFinished, doNotUnload);
        }
        
        #region Loader Methods

        private void Load(TI sceneItem, Action onFinished = null, bool doNotUnload = false)
        {
            IList<string> oldScenes = null;
            if (!doNotUnload)
            {
                oldScenes = new List<string>();
                for (var i = 0; i < SceneManager.sceneCount; i++)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    oldScenes.Add(scene.path);
                }
            }

            RaiseBlendEvent(RuntimeOnBlendSceneType.PreShowBlend, sceneItem.Identifier, () =>
            {
                _blending.ShowBlend(() =>
                {
                    RaiseBlendEvent(RuntimeOnBlendSceneType.PostShowBlend, sceneItem.Identifier,
                        () => DoLoadAsync(sceneItem, onFinished, oldScenes?.ToArray()));
                });
            });
        }

        private void DoLoadAsync(TI sceneItem, Action onFinished, string[] oldScenes)
        {
            StartCoroutine(ChangeScenes(
                () => RaiseSwitchEvent(RuntimeOnSwitchSceneType.UnloadScenes, sceneItem.Identifier, oldScenes),
                () => RaiseSwitchEvent(RuntimeOnSwitchSceneType.LoadScenes, sceneItem.Identifier, sceneItem.Scenes),
                () =>
                {
                    RaiseBlendEvent(RuntimeOnBlendSceneType.PreHideBlend, sceneItem.Identifier, () =>
                    {
                        _blending.HideBlend(() =>
                        {
                            RaiseBlendEvent(RuntimeOnBlendSceneType.PostHideBlend, sceneItem.Identifier, () =>
                            {
                                CurrentState = sceneItem.Identifier;
                                onFinished?.Invoke(); 
                            });
                        });
                    });
                }
            ));
        }

        #endregion
        
        protected IEnumerator ChangeScenes(Func<string[]> oldScenesGetter, Func<string[]> newScenesGetter, Action onFinished)
        {
            var oldScenes = oldScenesGetter();
            if (oldScenes != null && oldScenes.Length > 0)
            {
                foreach (var oldScene in oldScenes)
                {
                    SceneManager.UnloadSceneAsync(oldScene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
                }
            }

            var newScenes = newScenesGetter();
            var operations = new List<AsyncOperation>();
            foreach (var newScene in newScenes)
            {
                var operation = SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
                operation.allowSceneActivation = false;
                operation.completed += _ => SceneManager.SetActiveScene(SceneManager.GetSceneByPath(newScenes[0]));

                operations.Add(operation);
            }


            while (!operations.IsReady())
            {
                _blending.LoadingProgress = operations.CalculateProgress();
                yield return null;
            }

            foreach (var operation in operations)
            {
                operation.allowSceneActivation = true;
            }

            while (!operations.IsDone())
            {
                _blending.LoadingProgress = operations.CalculateProgress();
                yield return null;
            }

            onFinished?.Invoke();
        }
        
        protected virtual void RaiseBlendEvent(RuntimeOnBlendSceneType type, string identifier, Action asyncAction)
        {
            if (!UseBlendCallbacks || _blendCallbacks == null || !_blendCallbacks.ContainsKey(type) || _blendCallbacks[type].Count <= 0)
            {
                asyncAction?.Invoke();
                return;
            }

            var counter = new Decrementing(_blendCallbacks[type].Count);
            foreach (var methodInfo in _blendCallbacks[type])
            {
                Action action = () => counter.Try(asyncAction);
                methodInfo.Invoke(null, new object[] { new RuntimeOnBlendSceneArgs(identifier, action) });
            }
        }

        protected virtual string[] RaiseSwitchEvent(RuntimeOnSwitchSceneType type, string identifier, string[] scenes)
        {
            if (!UseSwitchCallbacks || _switchCallbacks == null || !_switchCallbacks.ContainsKey(type) || _switchCallbacks[type].Count <= 0)
                return scenes;

            var result = new List<string>(scenes);
            foreach (var methodInfo in _switchCallbacks[type])
            {
                var args = new RuntimeOnSwitchSceneArgs(identifier, scenes);
                methodInfo.Invoke(null, new object[] { args });
                result.AddRange(args.AdditionalScenes);
            }

            return result.ToArray();
        }

        protected abstract TI FindSceneItem(string identifier);
        protected abstract string GetAllowedParameterDataType(string identifier);
        protected abstract bool IsAllowNullParameterData(string identifier);
    }
}