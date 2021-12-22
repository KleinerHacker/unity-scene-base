using System;

namespace UnitySceneBase.Runtime.scene_system.scene_base.Scripts.Runtime.Types
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RuntimeOnBlendSceneAttribute : Attribute
    {
        public RuntimeOnBlendSceneType Type { get; }

        public RuntimeOnBlendSceneAttribute(RuntimeOnBlendSceneType type)
        {
            Type = type;
        }
    }

    public enum RuntimeOnBlendSceneType
    {
        PreShowBlend,
        PostShowBlend,
        PreHideBlend,
        PostHideBlend,
    }

    /// <summary>
    /// Event arguments for <see cref="RuntimeOnBlendSceneAttribute"/> methods. <b>You must always call callback action, otherwise system stocks!</b>
    /// </summary>
    public sealed class RuntimeOnBlendSceneArgs
    {
        /// <summary>
        /// Identifier of loading scene
        /// </summary>
        public string Identifier { get; }
        /// <summary>
        /// Callback to continue loading scene. <b>Must call from method, otherwise system stocks!</b>
        /// </summary>
        public Action Callback { get; }

        internal RuntimeOnBlendSceneArgs(string identifier, Action callback)
        {
            Identifier = identifier;
            Callback = callback;
        }
    }
}