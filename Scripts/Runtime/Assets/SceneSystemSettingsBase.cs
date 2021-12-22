using System;
using UnityBlending.Runtime.scene_system.blending.Scripts.Runtime.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

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

        #endregion
    }
    
    [Serializable]
    public abstract class SceneItemBase
    {
        #region Inspector Data

        [SerializeField]
        private string identifier;

        #endregion

        #region Properties

        public string Identifier => identifier;
        
        public abstract string[] Scenes { get; }

        #endregion
    }

    public enum SceneBlendState
    {
        Shown,
        Hidden,
    }
}