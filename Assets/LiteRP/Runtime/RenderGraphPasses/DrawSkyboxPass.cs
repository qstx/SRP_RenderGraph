using UnityEngine;
using LiteRP.FrameData;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public partial class LiteRenderGraphRecorder
    {
        private static readonly ProfilingSampler s_DrawSkyboxPassProfilingSampler=new ProfilingSampler("Draw Skybox");
        
        internal class DrawSkyboxPassData
        {
            internal RendererListHandle skyboxRendererListHandle;
        }

        private void AddDrawSkyboxPass(RenderGraph renderGraph, CameraData cameraData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<DrawSkyboxPassData>("Draw Skybox", out var passData, s_DrawSkyboxPassProfilingSampler))
            {
                passData.skyboxRendererListHandle = renderGraph.CreateSkyboxRendererList(cameraData.camera);
                builder.UseRendererList(passData.skyboxRendererListHandle);
                
                if(m_BackbufferColorHandle.IsValid())
                    builder.SetRenderAttachment(m_BackbufferColorHandle,0,AccessFlags.Write);
                
                builder.SetRenderFunc((DrawSkyboxPassData data, RasterGraphContext context) =>
                {
                    context.cmd.DrawRendererList(data.skyboxRendererListHandle);
                });
            }
        }
    }
}