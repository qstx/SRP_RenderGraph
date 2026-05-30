using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRenderGraphRecorder
    {
        private static readonly ProfilingSampler s_DrawObjectsProfilingSampler=new ProfilingSampler("Draw Objects");
        private static readonly ShaderTagId s_ShaderTagId = new ShaderTagId("SRPDefaultUnlit");
        internal class DrawObjectsPassData
        {
            internal TextureHandle backbufferHandle;
            internal RendererListHandle opaqueRendererListHandle;
            internal RendererListHandle transparentRendererListHandle;
        }

        private void AddDrawObjectsPass(RenderGraph renderGraph, ContextContainer frameData)
        {
            CameraData cameraData = frameData.Get<CameraData>();
            using (var builder = renderGraph.AddRasterRenderPass<DrawObjectsPassData>("Draw Objects Pass", out var passData, s_DrawObjectsProfilingSampler))
            {
                RendererListDesc opaqueRendererDesc = new RendererListDesc(s_ShaderTagId,cameraData.cullingResults,cameraData.camera);
                opaqueRendererDesc.sortingCriteria = SortingCriteria.CommonOpaque;
                opaqueRendererDesc.renderQueueRange = RenderQueueRange.opaque;
                passData.opaqueRendererListHandle=renderGraph.CreateRendererList(opaqueRendererDesc);
                builder.UseRendererList(passData.opaqueRendererListHandle);
                
                RendererListDesc transparentRendererDesc = new RendererListDesc(s_ShaderTagId,cameraData.cullingResults,cameraData.camera);
                transparentRendererDesc.sortingCriteria = SortingCriteria.CommonTransparent;
                transparentRendererDesc.renderQueueRange = RenderQueueRange.transparent;
                passData.transparentRendererListHandle=renderGraph.CreateRendererList(transparentRendererDesc);
                builder.UseRendererList(passData.transparentRendererListHandle);

                passData.backbufferHandle = renderGraph.ImportBackbuffer(BuiltinRenderTextureType.CurrentActive);
                builder.SetRenderAttachment(passData.backbufferHandle,0,AccessFlags.Write);
                
                builder.AllowPassCulling(false);
                
                builder.SetRenderFunc((DrawObjectsPassData passData, RasterGraphContext context) =>
                {
                    context.cmd.DrawRendererList(passData.opaqueRendererListHandle);
                    context.cmd.DrawRendererList(passData.transparentRendererListHandle);
                });
            }
        }
    }
    
}