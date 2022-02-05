using System.Linq;
using UnityCommonEx.Runtime.common_ex.Scripts.Runtime.Utils.Extensions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnitySceneBase.Runtime.scene_system.scene_base.Scripts.Runtime.Utils;

namespace UnitySceneBase.Editor.scene_system.scene_base.Scripts.Editor.Provider
{
    public abstract class ItemListBase : ReorderableList
    {
        private const float LeftMargin = 15f;
        private const float BottomMargin = 2f;
        private const float ColumnSpace = 5f;

        private const float KeyWidth = 300f;
        private const float UnloadWidth = 30f;
        private const float ParameterDataTypeWidth = 300f;
        private const float ParameterDataAllowNullWidth = 30f;
        private const float CommonWidth = KeyWidth + UnloadWidth + ParameterDataTypeWidth + ParameterDataAllowNullWidth;

        public ItemListBase(SerializedObject serializedObject, SerializedProperty elements) : base(serializedObject, elements)
        {
            drawHeaderCallback += DrawHeaderCallback;
            drawElementCallback += DrawElementCallback;
        }

        private void DrawHeaderCallback(Rect rect)
        {
            var pos = new Rect(rect.x + LeftMargin, rect.y, KeyWidth, rect.height);
            EditorGUI.LabelField(pos, "Identifier");

            var commonWidth = rect.width - (CommonWidth + LeftMargin);
            pos = new Rect(rect.x + LeftMargin + KeyWidth, rect.y, commonWidth, rect.height);
            OnDrawCommonHeader(pos);

            pos = new Rect(rect.x + LeftMargin + KeyWidth + commonWidth, rect.y, UnloadWidth, rect.height);
            EditorGUI.LabelField(pos, new GUIContent("NU", "Never Unload Scene"));
            
            pos = new Rect(rect.x + LeftMargin + KeyWidth + commonWidth + UnloadWidth, rect.y, ParameterDataTypeWidth, rect.height);
            EditorGUI.LabelField(pos, new GUIContent("Parameter Data Type", "Type that is allowed to use."));
            
            pos = new Rect(rect.x + LeftMargin + KeyWidth + commonWidth + UnloadWidth + ParameterDataTypeWidth, rect.y, ParameterDataAllowNullWidth, rect.height);
            EditorGUI.LabelField(pos, new GUIContent("AN", "Allow NULL for parameter data"));
        }

        private void DrawElementCallback(Rect rect, int i, bool isactive, bool isfocused)
        {
            var property = serializedProperty.GetArrayElementAtIndex(i);
            var identifierProperty = property.FindPropertyRelative("identifier");
            var neverUnloadProperty = property.FindPropertyRelative("neverUnload");
            var parameterDataTypeProperty = property.FindPropertyRelative("parameterDataType");
            var parameterDataAllowNullProperty = property.FindPropertyRelative("parameterDataAllowNull");

            var pos = new Rect(rect.x, rect.y, KeyWidth - ColumnSpace, rect.height - BottomMargin);
            EditorGUI.PropertyField(pos, identifierProperty, GUIContent.none);

            var commonWidth = rect.width - CommonWidth;
            pos = new Rect(rect.x + KeyWidth, rect.y, commonWidth - ColumnSpace, rect.height - BottomMargin);
            OnDrawCommonElement(pos, i, isactive, isfocused);

            pos = new Rect(rect.x + KeyWidth + commonWidth, rect.y, UnloadWidth - ColumnSpace, rect.height - BottomMargin);
            EditorGUI.PropertyField(pos, neverUnloadProperty, GUIContent.none);
            
            pos = new Rect(rect.x + KeyWidth + commonWidth + UnloadWidth, rect.y, ParameterDataTypeWidth - ColumnSpace, rect.height - BottomMargin);
            DrawParameterTypePopup(pos, parameterDataTypeProperty);
            
            pos = new Rect(rect.x + KeyWidth + commonWidth + UnloadWidth + ParameterDataTypeWidth, rect.y, ParameterDataAllowNullWidth - ColumnSpace, rect.height - BottomMargin);
            EditorGUI.PropertyField(pos, parameterDataAllowNullProperty, GUIContent.none);
        }

        private void DrawParameterTypePopup(Rect rect, SerializedProperty property)
        {
            var typeStr = property.stringValue;
            
            var index = EditorGUI.Popup(rect, ParameterDataUtils.ParameterTypes.IndexOf(x => x.FullName == typeStr), 
                ParameterDataUtils.ParameterTypes.Select(x => x.Name).ToArray());
            if (index < 0)
                return;
            
            property.stringValue = ParameterDataUtils.ParameterTypes[index].FullName;
        }

        protected abstract void OnDrawCommonHeader(Rect rect);
        protected abstract void OnDrawCommonElement(Rect rect, int i, bool isactive, bool isfocused);
    }
}