using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UnitySceneBase.Editor.scene_system.scene_base.Scripts.Editor.Provider
{
    public sealed class GameObjectItemList : ReorderableList
    {
        private const float LeftMargin = 15f;
        private const float BottomMargin = 2f;
        private const float ColumnSpace = 5f;

        private const float NameWidth = 300f;
        private const float CommonWidth = NameWidth;

        public GameObjectItemList(SerializedObject serializedObject, SerializedProperty elements) : base(serializedObject, elements)
        {
            drawHeaderCallback += DrawHeaderCallback;
            drawElementCallback += DrawElementCallback;
        }

        private void DrawHeaderCallback(Rect rect)
        {
            var pos = new Rect(rect.x + LeftMargin, rect.y, NameWidth, rect.height);
            EditorGUI.LabelField(pos, "Name");

            var prefabWidth = rect.width - (CommonWidth + LeftMargin);
            pos = new Rect(rect.x + LeftMargin + NameWidth, rect.y, prefabWidth, rect.height);
            EditorGUI.LabelField(pos, "Prefab");
        }

        private void DrawElementCallback(Rect rect, int i, bool isactive, bool isfocused)
        {
            var property = serializedProperty.GetArrayElementAtIndex(i);
            var nameProperty = property.FindPropertyRelative("objectName");
            var sceneProperty = property.FindPropertyRelative("prefab");

            var pos = new Rect(rect.x, rect.y, NameWidth - ColumnSpace, rect.height - BottomMargin);
            EditorGUI.PropertyField(pos, nameProperty, GUIContent.none);

            var prefabWidth = rect.width - CommonWidth;
            pos = new Rect(rect.x + NameWidth, rect.y, prefabWidth - ColumnSpace, rect.height - BottomMargin);
            EditorGUI.PropertyField(pos, sceneProperty, GUIContent.none);
        }
    }
}