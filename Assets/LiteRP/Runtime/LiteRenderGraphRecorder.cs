using System;
using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRenderGraphRecorder : IRenderGraphRecorder,IDisposable
    {
        private static readonly ShaderTagId s_ShaderTagId = new ShaderTagId("SRPDefaultUnlit");
        private TextureHandle m_BackbufferColorHandle = TextureHandle.nullHandle;
        private RTHandle m_TargetColorHandle = null;
        public void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            CameraData cameraData = frameData.Get<CameraData>();
            CreateRenderGraphCameraRenderTargets(renderGraph, cameraData);
            
            AddSetupCameraPropertiesPass(renderGraph, cameraData);
            CameraClearFlags clearFlags = cameraData.camera.clearFlags;
            if(!renderGraph.nativeRenderPassesEnabled && clearFlags!=CameraClearFlags.Nothing)
                AddClearRenderTargetPass(renderGraph, cameraData);
            
            AddDrawOpaqueObjectsPass(renderGraph, cameraData);
            
            if(cameraData.camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
                AddDrawSkyboxPass(renderGraph, cameraData);
            
            AddDrawTransparentObjectsPass(renderGraph, cameraData);
        }

        private void CreateRenderGraphCameraRenderTargets(RenderGraph renderGraph, CameraData cameraData)
        {
            RenderTargetIdentifier targetColorId = BuiltinRenderTextureType.CameraTarget;
            if (m_TargetColorHandle == null)
            {
                m_TargetColorHandle = RTHandles.Alloc(targetColorId, "Backbuffer Color");
            }
            
            Color cameraBackgroundColor = CoreUtils.ConvertSRGBToActiveColorSpace(cameraData.camera.backgroundColor);

            ImportResourceParams importBackbufferColorParams = new ImportResourceParams();
            bool clearOnFirstUse = !renderGraph.nativeRenderPassesEnabled;
            bool discardOnLastUse = !renderGraph.nativeRenderPassesEnabled;
            importBackbufferColorParams.clearOnFirstUse = clearOnFirstUse;
            importBackbufferColorParams.clearColor = cameraBackgroundColor;
            importBackbufferColorParams.discardOnLastUse = discardOnLastUse;
            
            bool colorRT_sRGB = (QualitySettings.activeColorSpace == ColorSpace.Linear);
            RenderTargetInfo importInfoColor = new RenderTargetInfo();
            importInfoColor.width=Screen.width;
            importInfoColor.height=Screen.height;
            importInfoColor.volumeDepth = 1;
            importInfoColor.msaaSamples = 1;
            importInfoColor.format = GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.Default, colorRT_sRGB);
            
            m_BackbufferColorHandle = renderGraph.ImportTexture(m_TargetColorHandle, importInfoColor, importBackbufferColorParams);
        }

        public void Dispose()
        {
            RTHandles.Release(m_TargetColorHandle);
            GC.SuppressFinalize(this);
        }
    }
}