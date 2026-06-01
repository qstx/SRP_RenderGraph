using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRenderGraphRecorder
    {
        private static readonly ProfilingSampler s_DrawOpaqueObjectsProfilingSampler=new ProfilingSampler("Draw Opaque Objects");
        internal class DrawOpaqueObjectsPassData
        {
            internal RendererListHandle opaqueRendererListHandle;
        }

        private void AddDrawOpaqueObjectsPass(RenderGraph renderGraph, CameraData cameraData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<DrawOpaqueObjectsPassData>("Draw Opaque Objects", out var passData, s_DrawOpaqueObjectsProfilingSampler))
            {
                RendererListDesc opaqueRendererDesc = new RendererListDesc(s_ShaderTagId,cameraData.cullingResults,cameraData.camera);
                opaqueRendererDesc.sortingCriteria = SortingCriteria.CommonOpaque;
                opaqueRendererDesc.renderQueueRange = RenderQueueRange.opaque;
                passData.opaqueRendererListHandle=renderGraph.CreateRendererList(opaqueRendererDesc);
                builder.UseRendererList(passData.opaqueRendererListHandle);

                if(m_BackbufferColorHandle.IsValid())
                    builder.SetRenderAttachment(m_BackbufferColorHandle,0,AccessFlags.Write);
                if(m_BackbufferDepthHandle.IsValid())
                    builder.SetRenderAttachmentDepth(m_BackbufferDepthHandle, AccessFlags.Write);
                
                builder.AllowPassCulling(false);
                
                builder.SetRenderFunc((DrawOpaqueObjectsPassData data, RasterGraphContext context) =>
                {
                    context.cmd.DrawRendererList(data.opaqueRendererListHandle);
                });
            }
        }
    }
}