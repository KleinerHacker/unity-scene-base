using System.Linq;
using UnityCommonEx.Runtime.common_ex.Scripts.Runtime.Utils.Extensions;
using UnityEditor;
using UnityEditor.Build;
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
            PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), out var symbols);
            var verbose = symbols.Contains(UnitySceneBaseEditorConstants.Building.Symbol.Verbose);
            var newVerbose = GUILayout.Toggle(verbose, guiContent, style);
            if (verbose != newVerbose)
            {
                if (newVerbose)
                {
                    PlayerSettings.SetScriptingDefineSymbols(
                        NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup),
                        symbols.Append(UnitySceneBaseEditorConstants.Building.Symbol.Verbose).ToArray()
                    );
                }
                else
                {
                    PlayerSettings.SetScriptingDefineSymbols(
                        NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup),
                        symbols.Remove(UnitySceneBaseEditorConstants.Building.Symbol.Verbose).ToArray()
                    );
                }
            }
        }
    }
}