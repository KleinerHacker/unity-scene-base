using System.Linq;
using UnityCommonEx.Runtime.common_ex.Scripts.Runtime.Utils.Extensions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditorEx.Editor.editor_ex.Scripts.Editor.Utils;
using UnityEngine;

namespace UnitySceneBase.Editor.scene_system.scene_base.Scripts.Editor.Utils
{
    public static class EditorGUILayoutEx
    {
        public static void SceneVerbose(GUIContent guiContent)
        {
            SceneVerbose(guiContent, GUI.skin.toggle);
        }

        public static void SceneVerbose(GUIContent guiContent, GUIStyle style)
        {
            var verbose = PlayerSettingsEx.IsScriptingSymbolDefined(UnitySceneBaseEditorConstants.Building.Symbol.Verbose);
            var newVerbose = GUILayout.Toggle(verbose, guiContent, style);
            if (verbose != newVerbose)
            {
                if (newVerbose)
                {
                    PlayerSettingsEx.AddScriptingSymbol(UnitySceneBaseEditorConstants.Building.Symbol.Verbose);
                }
                else
                {
                    PlayerSettingsEx.RemoveScriptingSymbol(UnitySceneBaseEditorConstants.Building.Symbol.Verbose);
                }
            }
        }
    }
}