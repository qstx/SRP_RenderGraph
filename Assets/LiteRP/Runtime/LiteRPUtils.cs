using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP
{
    public static class LiteRPUtils
    {
        public static bool CanNativeRenderPassesEnabled()
        {
            return SystemInfo.graphicsDeviceType != GraphicsDeviceType.Direct3D12 &&
                   SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES3 &&
                   SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLCore;
        }
    }
}