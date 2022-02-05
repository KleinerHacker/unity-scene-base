using System;
using UnityBlending.Runtime.scene_system.blending.Scripts.Runtime.Components;
using UnityEngine;

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
        /// <summary>
        /// Behavior to use for animation inside callback
        /// </summary>
        public MonoBehaviour AnimationBehavior { get; }
        /// <summary>
        /// Blending system is worked on
        /// </summary>
        public BlendingSystem BlendingSystem { get; }

        internal RuntimeOnBlendSceneArgs(string identifier, Action callback, MonoBehaviour animationBehavior, BlendingSystem blendingSystem)
        {
            Identifier = identifier;
            Callback = callback;
            AnimationBehavior = animationBehavior;
            BlendingSystem = blendingSystem;
        }
    }
}