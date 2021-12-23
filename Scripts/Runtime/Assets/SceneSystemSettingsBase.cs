using System;
using System.Linq;
using System.Reflection;
using UnityBlending.Runtime.scene_system.blending.Scripts.Runtime.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnitySceneBase.Runtime.scene_system.scene_base.Scripts.Runtime.Types;
using UnitySceneBase.Runtime.scene_system.scene_base.Scripts.Runtime.Utils;

namespace UnitySceneBase.Runtime.scene_system.scene_base.Scripts.Runtime.Assets
{
    public abstract class SceneSystemSettingsBase<T> : ScriptableObject where T : SceneItemBase
    {
        #region Inspector Data

        [SerializeField]
        private bool useSystem = true;

        [SerializeField]
        private T[] items = Array.Empty<T>();

        [SerializeField]
        private bool createEventSystem = true;

        [SerializeField]
        private bool esUseNavigation = true;

        [SerializeField]
        private GameObject esFirstSelection;

        [SerializeField]
        private int esDragThreshold = 10;

        [SerializeField]
        private float esMoveRepeatDelay = 0.5f;

        [SerializeField]
        private float esMoveRepeatRate = 0.1f;

        [SerializeField]
        private bool esDeselectOnBackground = true;

        [SerializeField]
        private UIPointerBehavior esPointerBehavior = UIPointerBehavior.SingleMouseOrPenButMultiTouchAndTrack;

        [SerializeField]
        private InputActionAsset esActionAsset;

        [SerializeField]
        private Transform esXROrigin;

        [SerializeField]
        private BlendingSystem blendingSystem;

        [SerializeField]
        [Tooltip("State of blending on startup of game")]
        private SceneBlendState startupBlendState = SceneBlendState.Shown;

        [SerializeField]
        private bool useBlendCallbacks;

        [SerializeField]
        private bool useSwitchCallbacks = true;

        [SerializeField]
        private GameObjectItem[] additionalGameObjects;

        [SerializeField]
        private ScriptableObject[] parameterInitialData = new ScriptableObject[0];

        #endregion

        #region Properties

        public bool UseSystem => useSystem;

        public T[] Items => items;

        public bool CreateEventSystem => createEventSystem;

        public bool ESUseNavigation => esUseNavigation;

        public GameObject ESFirstSelection => esFirstSelection;

        public float ESMoveRepeatDelay => esMoveRepeatDelay;

        public float ESMoveRepeatRate => esMoveRepeatRate;

        public bool ESDeselectOnBackground => esDeselectOnBackground;

        public int ESDragThreshold => esDragThreshold;

        public UIPointerBehavior ESPointerBehavior => esPointerBehavior;

        public InputActionAsset ESActionAsset => esActionAsset;

        public Transform ESXROrigin => esXROrigin;

        public GameObject BlendingSystem => blendingSystem?.gameObject;

        public SceneBlendState StartupBlendState => startupBlendState;

        public bool UseBlendCallbacks => useBlendCallbacks;

        public bool UseSwitchCallbacks => useSwitchCallbacks;

        public GameObjectItem[] AdditionalGameObjects => additionalGameObjects;

        public ScriptableObject[] ParameterInitialData => parameterInitialData;

        #endregion

        #region Builtin Methods

#if UNITY_EDITOR
        private void OnValidate()
        {
            foreach (var parameterType in ParameterDataUtils.ParameterTypes)
            {
                var attribute = parameterType.GetCustomAttribute<ParameterInitialDataTypeAttribute>();
                if (attribute == null)
                    continue;
                
                if (ParameterInitialData.Select(x => x.GetType()).Contains(attribute.Type))
                    continue;
                
                var scriptableObject = CreateInstance(attribute.Type);
                AssetDatabase.CreateAsset(scriptableObject, "Assets/Resources/" + parameterType.Name + ".asset");

                parameterInitialData = parameterInitialData.Append(scriptableObject).ToArray();
            }
        }
#endif
        
        #endregion
    }

    [Serializable]
    public abstract class SceneItemBase
    {
        #region Inspector Data

        [SerializeField]
        private string identifier;

        [SerializeField]
        private string parameterDataType = typeof(ParameterData).FullName;

        [SerializeField]
        private bool parameterDataAllowNull = true;

        [SerializeField]
        private bool neverUnload;

        #endregion

        #region Properties

        public string Identifier => identifier;

        public string ParameterDataType => parameterDataType;

        public bool ParameterDataAllowNull => parameterDataAllowNull;

        public bool NeverUnload => neverUnload;

        public abstract string[] Scenes { get; }

        #endregion
    }

    [Serializable]
    public sealed class GameObjectItem
    {
        #region Inspector Data

        [SerializeField]
        private string objectName;

        [SerializeField]
        private GameObject prefab;

        #endregion

        #region Properties

        public string ObjectName => objectName;

        public GameObject Prefab => prefab;

        #endregion
    }

    public enum SceneBlendState
    {
        Shown,
        Hidden,
    }
}