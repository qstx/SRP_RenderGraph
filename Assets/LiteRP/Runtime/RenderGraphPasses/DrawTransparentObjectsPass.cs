using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRenderGraphRecorder
    {
        private static readonly ProfilingSampler s_DrawTransparentObjectsProfilingSampler=new ProfilingSampler("Draw Transparent Objects");
        internal class DrawTransparentObjectsPassData
        {
            internal RendererListHandle transparentRendererListHandle;
        }

        private void AddDrawTransparentObjectsPass(RenderGraph renderGraph, CameraData cameraData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<DrawTransparentObjectsPassData>("Draw Transparent Objects Pass", out var passData, s_DrawTransparentObjectsProfilingSampler))
            {
                RendererListDesc transparentRendererDesc = new RendererListDesc(s_ShaderTagId,cameraData.cullingResults,cameraData.camera);
                transparentRendererDesc.sortingCriteria = SortingCriteria.CommonTransparent;
                transparentRendererDesc.renderQueueRange = RenderQueueRange.transparent;
                passData.transparentRendererListHandle=renderGraph.CreateRendererList(transparentRendererDesc);
                builder.UseRendererList(passData.transparentRendererListHandle);

                if(m_BackbufferColorHandle.IsValid())
                    builder.SetRenderAttachment(m_BackbufferColorHandle,0,AccessFlags.Write);
                
                builder.AllowPassCulling(false);
                
                builder.SetRenderFunc((DrawTransparentObjectsPassData data, RasterGraphContext context) =>
                {
                    context.cmd.DrawRendererList(data.transparentRendererListHandle);
                });
            }
        }
    }
}