using System.Collections.Generic;
using UnityEditor;
using UnityEditorEx.Editor.editor_ex.Scripts.Editor.Utils;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using UnitySceneBase.Runtime.scene_system.scene_base.Scripts.Runtime.Types;

namespace UnitySceneBase.Editor.scene_system.scene_base.Scripts.Editor.Provider
{
    public abstract class SceneSettingsProviderBase : SettingsProvider
    {
        #region Properties

        protected abstract SerializedObject Settings { get; }

        protected abstract bool HasAnyEmptyIdentifier { get; }
        protected abstract bool HasAnyDoubleIdentifier { get; }

        #endregion

        private SerializedObject _settings;
        private SerializedProperty _useSystemProperty;
        private SerializedProperty _itemsProperty;
        private SerializedProperty _blendingProperty;
        private SerializedProperty _startupBlendingStateProperty;
        private SerializedProperty _createESProperty;
        private SerializedProperty _esNavigationProperty;
        private SerializedProperty _esFirstSelectionProperty;
        private SerializedProperty _esDragThresholdProperty;
        private SerializedProperty _esRepeatDelayProperty;
        private SerializedProperty _esRepeatRateProperty;
        private SerializedProperty _esDeselectOnBackgroundProperty;
        private SerializedProperty _esPointerBehaviorProperty;
        private SerializedProperty _esActionAssetProperty;
        private SerializedProperty _esXROriginProperty;
        private SerializedProperty _useBlendCallbacksProperty;
        private SerializedProperty _useSwitchCallbacksProperty;
        private SerializedProperty _additionalGameObjectsProperty;
        private SerializedProperty _parameterInitialDataProperty;

        private bool _foldEventSystem;
        private readonly IDictionary<string, bool> _foldInitData = new Dictionary<string, bool>();

        private ReorderableList _sceneItemList;
        private ReorderableList _gameObjectItemList;

        protected SceneSettingsProviderBase(string path, IEnumerable<string> keywords = null) : base(path, SettingsScope.Project, keywords)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _settings = Settings;
            if (_settings == null)
                return;

            _useSystemProperty = _settings.FindProperty("useSystem");
            _itemsProperty = _settings.FindProperty("items");
            _blendingProperty = _settings.FindProperty("blendingSystem");
            _startupBlendingStateProperty = _settings.FindProperty("startupBlendState");
            _createESProperty = _settings.FindProperty("createEventSystem");
            _esNavigationProperty = _settings.FindProperty("esUseNavigation");
            _esFirstSelectionProperty = _settings.FindProperty("esFirstSelection");
            _esDragThresholdProperty = _settings.FindProperty("esDragThreshold");
            _esRepeatDelayProperty = _settings.FindProperty("esMoveRepeatDelay");
            _esRepeatRateProperty = _settings.FindProperty("esMoveRepeatRate");
            _esDeselectOnBackgroundProperty = _settings.FindProperty("esDeselectOnBackground");
            _esPointerBehaviorProperty = _settings.FindProperty("esPointerBehavior");
            _esActionAssetProperty = _settings.FindProperty("esActionAsset");
            _esXROriginProperty = _settings.FindProperty("esXROrigin");
            _useBlendCallbacksProperty = _settings.FindProperty("useBlendCallbacks");
            _useSwitchCallbacksProperty = _settings.FindProperty("useSwitchCallbacks");
            _additionalGameObjectsProperty = _settings.FindProperty("additionalGameObjects");
            _parameterInitialDataProperty = _settings.FindProperty("parameterInitialData");

            _sceneItemList = CreateItemList(_settings, _itemsProperty);
            _gameObjectItemList = new GameObjectItemList(_settings, _additionalGameObjectsProperty);
        }

        public override void OnGUI(string searchContext)
        {
            _settings.Update();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_useSystemProperty, new GUIContent("Use System"));
            ExtendedEditorGUILayout.SymbolField("Editor Scene Loading", UnitySceneBaseEditorConstants.Building.Symbol.EditorSceneLoading);
            ExtendedEditorGUILayout.SymbolField("Verbose Logging", UnitySceneBaseEditorConstants.Building.Symbol.Verbose);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(!_useSystemProperty.boolValue);
            {
                if (HasAnyEmptyIdentifier)
                {
                    EditorGUILayout.HelpBox("There are identifiers with no content", MessageType.Warning);
                }

                if (HasAnyDoubleIdentifier)
                {
                    EditorGUILayout.HelpBox("There are double identifiers in list", MessageType.Warning);
                }

                EditorGUILayout.PropertyField(_blendingProperty);
                EditorGUILayout.PropertyField(_startupBlendingStateProperty);
                _sceneItemList.DoLayoutList();

                _foldEventSystem = EditorGUILayout.BeginFoldoutHeaderGroup(_foldEventSystem, "Event System");
                if (_foldEventSystem)
                {
                    EditorGUI.indentLevel = 1;

                    EditorGUILayout.PropertyField(_createESProperty);
                    EditorGUILayout.Space();

                    EditorGUI.BeginDisabledGroup(!_createESProperty.boolValue);

                    EditorGUILayout.PropertyField(_esNavigationProperty, new GUIContent("Use navigation system"));
                    EditorGUILayout.PropertyField(_esFirstSelectionProperty, new GUIContent("First selected UI object"));
                    EditorGUILayout.PropertyField(_esDragThresholdProperty, new GUIContent("Drag Threshold"));
                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(_esRepeatDelayProperty, new GUIContent("Move Repeat Delay"));
                    EditorGUILayout.PropertyField(_esRepeatRateProperty, new GUIContent("Move Repeat Rate"));
                    EditorGUILayout.PropertyField(_esDeselectOnBackgroundProperty, new GUIContent("Deselect On Background"));
                    EditorGUILayout.PropertyField(_esPointerBehaviorProperty, new GUIContent("Pointer Behavior"));
                    EditorGUILayout.PropertyField(_esActionAssetProperty, new GUIContent("Action Asset", "Overwrite default action asset. Leave empty to use default action asset."));
                    EditorGUILayout.PropertyField(_esXROriginProperty, new GUIContent("XR Origin"));

                    EditorGUI.EndDisabledGroup();

                    EditorGUI.indentLevel = 0;
                }

                EditorGUILayout.EndFoldoutHeaderGroup();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Scene Events", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(_useBlendCallbacksProperty, new GUIContent("Use Blending Callback Events", "Use callback events with attribute " + nameof(RuntimeOnBlendSceneAttribute)));
                EditorGUILayout.PropertyField(_useSwitchCallbacksProperty, new GUIContent("Use Scene Switch Callback Events", "Use callback events with attribute " + nameof(RuntimeOnSwitchSceneAttribute)));

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Additional custom game objects", EditorStyles.boldLabel);
                _gameObjectItemList.DoLayoutList();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Initial Data for Parameters", EditorStyles.boldLabel);
                for (var i = 0; i < _parameterInitialDataProperty.arraySize; i++)
                {
                    var parameterInitData = _parameterInitialDataProperty.GetArrayElementAtIndex(i);
                    DrawParameterInitialDataSection(parameterInitData);
                }
            }
            EditorGUI.EndDisabledGroup();

            _settings.ApplyModifiedProperties();
        }

        private void DrawParameterInitialDataSection(SerializedProperty parameterInitData)
        {
            var scriptableObject = (ScriptableObject)parameterInitData.objectReferenceValue;
            var name = scriptableObject.name;

            if (!_foldInitData.ContainsKey(name))
            {
                _foldInitData.Add(name, false);
            }

            EditorGUILayout.Space();
            _foldInitData[name] = EditorGUILayout.BeginFoldoutHeaderGroup(_foldInitData[name], name);
            if (_foldInitData[name])
            {
                EditorGUI.indentLevel = 1;
                UnityEditor.Editor editor = null;
                UnityEditor.Editor.CreateCachedEditor(scriptableObject, null, ref editor);
                editor.OnInspectorGUI();
                EditorGUI.indentLevel = 0;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        protected abstract ReorderableList CreateItemList(SerializedObject settings, SerializedProperty itemsProperty);
    }
}