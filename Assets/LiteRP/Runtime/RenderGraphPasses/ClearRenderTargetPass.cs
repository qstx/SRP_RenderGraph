using UnityEngine;
using LiteRP.FrameData;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRenderGraphRecorder
    {
        private static readonly ProfilingSampler s_ClearRenderTargetProfilingSampler=new ProfilingSampler("Clear Render Target");
        
        internal class ClearRenderTargetPassData
        {
            internal RTClearFlags clearFlag;
            internal Color clearColor;
        }

        private void AddClearRenderTargetPass(RenderGraph renderGraph, CameraData cameraData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<ClearRenderTargetPassData>("Clear Render Target", out var passData, s_ClearRenderTargetProfilingSampler))
            {
                passData.clearFlag = cameraData.GetClearFlag();
                passData.clearColor = cameraData.GetClearColor();
                if(m_BackbufferColorHandle.IsValid())
                    builder.SetRenderAttachment(m_BackbufferColorHandle, 0, AccessFlags.Write);
                builder.AllowPassCulling(false);
                builder.SetRenderFunc((ClearRenderTargetPassData data, RasterGraphContext context) =>
                {
                    context.cmd.ClearRenderTarget(data.clearFlag, data.clearColor,1,0);
                });
            }
        }
    }
}