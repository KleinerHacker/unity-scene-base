using System;
using System.Linq;
using UnitySceneBase.Runtime.scene_system.scene_base.Scripts.Runtime.Types;

namespace UnitySceneBase.Runtime.scene_system.scene_base.Scripts.Runtime.Utils
{
    public static class ParameterDataUtils
    {
        public static Type[] ParameterTypes { get; }

        static ParameterDataUtils()
        {
            ParameterTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof(ParameterData).IsAssignableFrom(x))
                .ToArray();
        }
    }
}