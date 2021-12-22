using UnityExtension.Runtime.extension.Scripts.Runtime.Components;
using UnitySceneBase.Runtime.scene_system.scene_base.Scripts.Runtime.Types;

namespace UnitySceneBase.Runtime.scene_system.scene_base.Scripts.Runtime.Components
{
    public sealed class SceneParameterSystem : SearchingSingletonBehavior<SceneParameterSystem>
    {
        internal static void UpdateParameterData(ParameterData parameterData, bool overwrite = true)
        {
            Singleton._parameterData.Update(parameterData, overwrite);
        }
        
        private readonly ParameterData _parameterData = new ParameterData();
    }
}