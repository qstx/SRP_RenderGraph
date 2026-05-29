using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP
{
    public class LiteRenderPipeline : RenderPipeline
    {
        private readonly static ShaderTagId s_ShaderTagId = new ShaderTagId("SRPDefaultUnlit");
        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            BeginContextRendering(context, cameras);
            for (int i = 0; i < cameras.Count; i++)
            {
                Camera camera = cameras[i];
                RenderCamera(context, camera);
            }
            EndContextRendering(context, cameras);
        }

        private void RenderCamera(ScriptableRenderContext context, Camera camera)
        {
            BeginCameraRendering(context, camera);
            
            ScriptableCullingParameters cullingParameters = new ScriptableCullingParameters();
            if(!camera.TryGetCullingParameters(out cullingParameters))
                return;
            CullingResults cullingResults = context.Cull(ref cullingParameters);
            
            CommandBuffer cmd = CommandBufferPool.Get(camera.name);
            context.SetupCameraProperties(camera);
            
            bool clearSkybox = camera.clearFlags == CameraClearFlags.Skybox;
            bool clearDepth = camera.depth == 0;
            bool clearColor=camera.clearFlags == CameraClearFlags.Color;
            cmd.ClearRenderTarget(true,true, CoreUtils.ConvertSRGBToActiveColorSpace(camera.backgroundColor));

            if (clearSkybox)
            {
                var skyboxRenderer = context.CreateSkyboxRendererList(camera);
                cmd.DrawRendererList(skyboxRenderer);
            }
            
            var sortingSettings = new SortingSettings(camera);
            var drawSettings = new DrawingSettings(s_ShaderTagId,sortingSettings);
            var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            var rendererListParams = new RendererListParams(cullingResults, drawSettings, filteringSettings);
            var rendererList = context.CreateRendererList(ref rendererListParams); 
            cmd.DrawRendererList(rendererList);

            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            filteringSettings = new FilteringSettings(RenderQueueRange.transparent);
            rendererListParams = new RendererListParams(cullingResults, drawSettings, filteringSettings);
            rendererList = context.CreateRendererList(ref rendererListParams); 
            cmd.DrawRendererList(rendererList);
            
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
            
            context.Submit();
            
            EndCameraRendering(context, camera);
        }
    }
}