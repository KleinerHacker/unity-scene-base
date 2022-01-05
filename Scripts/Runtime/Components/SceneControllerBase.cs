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
    public abstract class SceneControllerBase<T, TI> : SearchingSingletonBehavior<T> where T : SceneControllerBase<T, TI> where TI : SceneItemBase
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
            goParameterSystem.AddComponent<SceneParameterController>();
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
            {
                Debug.LogWarning("[SceneSystem] Unable to find blending system. Game uses no blending between scenes!");
            }
            else
            {
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

        public abstract void Load(string identifier, bool doNotUnload, Action onFinished, ParameterData parameterData = null, bool overwrite = true);

        protected void Load(string identifier, bool doNotUnload, Action onFinished, ParameterData parameterData, bool overwrite, ScriptableObject[] scriptableObjects)
        {
#if SCENE_VERBOSE
            Debug.Log("[SceneSystem] Load scene(s) for " + identifier + " with parameter data " + parameterData);
#endif

            var sceneItem = FindSceneItem(identifier);
            if (sceneItem == null)
                throw new InvalidOperationException("Unable to find scene with identifier " + identifier);

            if (parameterData == null && !IsAllowNullParameterData(identifier))
                throw new InvalidOperationException("Parameter data with NULL value not allowed for " + identifier);
            var parameterDataType = GetAllowedParameterDataType(identifier);
            if (parameterData != null && parameterData.GetType().FullName != parameterDataType)
                throw new InvalidOperationException("Parameter data must of type " + parameterDataType + " for " + identifier);

            SceneParameterController.UpdateParameterData(parameterData, scriptableObjects, overwrite);
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
                if (_blending != null)
                {
                    _blending.ShowBlend(() =>
                    {
                        RaiseBlendEvent(RuntimeOnBlendSceneType.PostShowBlend, sceneItem.Identifier,
                            () => DoLoadAsync(sceneItem, onFinished, oldScenes?.ToArray()));
                    });
                }
                else
                {
                    RaiseBlendEvent(RuntimeOnBlendSceneType.PostShowBlend, sceneItem.Identifier,
                        () => DoLoadAsync(sceneItem, onFinished, oldScenes?.ToArray()));
                }
            });
        }

        private void DoLoadAsync(TI sceneItem, Action onFinished, string[] oldScenes)
        {
            StartCoroutine(ChangeScenes(
                () => RaiseSwitchEvent(RuntimeOnSwitchSceneType.UnloadScenes, CurrentState, oldScenes),
                () => RaiseSwitchEvent(RuntimeOnSwitchSceneType.LoadScenes, sceneItem.Identifier, sceneItem.Scenes),
                () =>
                {
                    RaiseBlendEvent(RuntimeOnBlendSceneType.PreHideBlend, sceneItem.Identifier, () =>
                    {
                        if (_blending != null)
                        {
                            _blending.HideBlend(() =>
                            {
                                RaiseBlendEvent(RuntimeOnBlendSceneType.PostHideBlend, sceneItem.Identifier, () =>
                                {
                                    CurrentState = sceneItem.Identifier;
                                    onFinished?.Invoke();
                                });
                            });
                        }
                        else
                        {
                            RaiseBlendEvent(RuntimeOnBlendSceneType.PostHideBlend, sceneItem.Identifier, () =>
                            {
                                CurrentState = sceneItem.Identifier;
                                onFinished?.Invoke();
                            });
                        }
                    });
                }
            ));
        }

        #endregion

        protected IEnumerator ChangeScenes(Func<string[]> oldScenesGetter, Func<string[]> newScenesGetter, Action onFinished)
        {
            var oldScenes = oldScenesGetter();
            //If all scenes must unload (this is not be able cause one scene must exists anymore). In this case a complete reload is required.
            var requiresCompleteLoad = (oldScenes?.Length ?? 0) >= SceneManager.sceneCount;
#if SCENE_VERBOSE
            Debug.Log("[SceneSystem] Change scene requires complete reload? " + requiresCompleteLoad);
#endif
            if (!requiresCompleteLoad)
            {
                if (oldScenes != null && oldScenes.Length > 0)
                {
#if SCENE_VERBOSE
                    Debug.Log("[SceneSystem] Unload scenes: " + string.Join(',', oldScenes));
#endif
                    foreach (var oldScene in oldScenes)
                    {
                        SceneManager.UnloadSceneAsync(oldScene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
                    }
                }
            }

            var newScenes = newScenesGetter();
#if SCENE_VERBOSE
            Debug.Log("[SceneSystem] Load scenes: " + string.Join(',', newScenes));
#endif
            var operations = new List<AsyncOperation>();
            for (var i = 0; i < newScenes.Length; i++)
            {
                var newScene = newScenes[i];

                AsyncOperation operation;
                if (requiresCompleteLoad && i == 0)
                {
#if SCENE_VERBOSE
                    Debug.Log("[SceneSystem] Load first scene as single");
#endif
                    operation = SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Single);
                }
                else
                {
#if SCENE_VERBOSE
                    Debug.Log("[SceneSystem] Load scene as additive");
#endif
                    operation = SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
                }

                operation.allowSceneActivation = false;
                operation.completed += _ => SceneManager.SetActiveScene(SceneManager.GetSceneByPath(newScenes[0]));

                operations.Add(operation);

                if (requiresCompleteLoad && i == 0)
                {
#if SCENE_VERBOSE
                    Debug.Log("[SceneSystem] Wait for first scene is loaded completely");
#endif
                    var waitForReadySingle = WaitForReady(operations);
                    while (waitForReadySingle.MoveNext()) yield return waitForReadySingle.Current;

                    operation.allowSceneActivation = true;

#if SCENE_VERBOSE
                    Debug.Log("[SceneSystem] Wait for first scene is done");
#endif
                    var waitForDoneSingle = WaitForDone(operations);
                    while (waitForDoneSingle.MoveNext()) yield return waitForDoneSingle.Current;
                }
            }

            var waitForReady = WaitForReady(operations);
            while (waitForReady.MoveNext()) yield return waitForReady.Current;

            foreach (var operation in operations)
            {
                operation.allowSceneActivation = true;
            }

            var waitForDone = WaitForDone(operations);
            while (waitForDone.MoveNext()) yield return waitForDone.Current;

#if SCENE_VERBOSE
            Debug.Log("[SceneSystem] Scene loading finished");
#endif
            onFinished?.Invoke();
        }

        private IEnumerator WaitForReady(List<AsyncOperation> operations)
        {
#if SCENE_VERBOSE
            Debug.Log("[SceneSystem] Wait for scene loading");
#endif
            while (!operations.IsReady())
            {
#if SCENE_VERBOSE
                Debug.Log("[SceneSystem] State: " + operations.CalculateProgress() + " -> " + string.Join('|', operations.Select(x => x.progress)));
#endif
                if (_blending != null)
                {
                    _blending.LoadingProgress = operations.CalculateProgress();
                }

                yield return null;
            }
        }

        private IEnumerator WaitForDone(List<AsyncOperation> operations)
        {
#if SCENE_VERBOSE
            Debug.Log("[SceneSystem] Wait for scene is done");
#endif
            while (!operations.IsDone())
            {
#if SCENE_VERBOSE
                Debug.Log("[SceneSystem] State: " + operations.CalculateProgress() + " -> " + string.Join('|', operations.Select(x => x.progress)));
#endif
                if (_blending != null)
                {
                    _blending.LoadingProgress = operations.CalculateProgress();
                }

                yield return null;
            }
        }

        protected virtual void RaiseBlendEvent(RuntimeOnBlendSceneType type, string identifier, Action asyncAction)
        {
            if (!UseBlendCallbacks || _blendCallbacks == null || !_blendCallbacks.ContainsKey(type) || _blendCallbacks[type].Count <= 0)
            {
#if SCENE_VERBOSE
                Debug.Log("[SceneSystem] No blend events found in game, " + type + " for " + identifier);
#endif
                asyncAction?.Invoke();
                return;
            }

#if SCENE_VERBOSE
            Debug.Log("[SceneSystem] Raise blend events " + type + " for " + identifier);
#endif
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
            {
#if SCENE_VERBOSE
                Debug.Log("[SceneSystem] No scene switch events found in game, " + type + " for " + identifier);
#endif
                return scenes;
            }

#if SCENE_VERBOSE
            Debug.Log("[SceneSystem] Raise scene switch events " + type + " for " + identifier);
#endif
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