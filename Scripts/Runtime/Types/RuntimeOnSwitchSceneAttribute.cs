using System;

namespace UnitySceneBase.Runtime.scene_system.scene_base.Scripts.Runtime.Types
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RuntimeOnSwitchSceneAttribute : Attribute
    {
        public RuntimeOnSwitchSceneType Type { get; }

        public RuntimeOnSwitchSceneAttribute(RuntimeOnSwitchSceneType type)
        {
            Type = type;
        }
    }

    public enum RuntimeOnSwitchSceneType
    {
        LoadScenes,
        UnloadScenes
    }
    
    /// <summary>
    /// Event arguments for <see cref="RuntimeOnBlendSceneAttribute"/> methods. <b>You must always call callback action, otherwise system stocks!</b>
    /// </summary>
    public sealed class RuntimeOnSwitchSceneArgs
    {
        /// <summary>
        /// Identifier of loading scene
        /// </summary>
        public string Identifier { get; }
        /// <summary>
        /// Scenes to load / unload
        /// </summary>
        public string[] Scenes { get; }

        /// <summary>
        /// Additional scenes to load / unload
        /// </summary>
        public string[] AdditionalScenes { get; set; }

        internal RuntimeOnSwitchSceneArgs(string identifier, string[] scenes)
        {
            Identifier = identifier;
            Scenes = scenes;
            AdditionalScenes = Array.Empty<string>();
        }
    }
}