using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP
{
    public class LiteRenderPipeline : RenderPipeline
    {
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
            
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
            
            context.Submit();
            
            EndCameraRendering(context, camera);
        }
    }
}