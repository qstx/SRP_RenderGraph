using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP.RenderGraphPasses
{
    public partial class LiteRenderGraphRecorder
    {
        private readonly static ProfilingSampler s_DrawObjectsProfilingSimplers=new ProfilingSampler("Draw Objects");
        internal class DrawObjectsPassData
        {
            
        }

        private void AddDrawObjectsPass(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<DrawObjectsPassData>("Draw Objects Pass", out var passData, s_DrawObjectsProfilingSimplers))
            {
                builder.SetRenderFunc((DrawObjectsPassData passData, RasterGraphContext context) =>
                {
                    
                });
            }
        }
    }
    
}