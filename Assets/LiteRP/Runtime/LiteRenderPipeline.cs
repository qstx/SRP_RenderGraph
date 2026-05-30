using System;
using System.Collections.Generic;
using LiteRP.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP
{
    public class LiteRenderPipeline : RenderPipeline
    {
        private static readonly ShaderTagId s_ShaderTagId = new ShaderTagId("SRPDefaultUnlit");
        
        private RenderGraph m_RenderGraph = null;
        private LiteRenderGraphRecorder m_LiteRenderGraphRecorder = null;
        private ContextContainer m_ContextContainer = null;

        public LiteRenderPipeline()
        {
            InitializeRenderGraph();
        }

        protected override void Dispose(bool disposing)
        {
            CleanupRenderGraph();
            base.Dispose(disposing);
        }

        private void InitializeRenderGraph()
        {
            m_RenderGraph = new RenderGraph("LiteRPRenderGraph");
            m_LiteRenderGraphRecorder = new LiteRenderGraphRecorder();
            m_ContextContainer = new ContextContainer();
        }

        private void CleanupRenderGraph()
        {
            m_ContextContainer?.Dispose();
            m_ContextContainer = null;
            m_LiteRenderGraphRecorder = null;
            m_RenderGraph?.Cleanup();
            m_RenderGraph = null;
        }

        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            BeginContextRendering(context, cameras);
            for (int i = 0; i < cameras.Count; i++)
            {
                Camera camera = cameras[i];
                RenderCamera(context, camera);
            }
            m_RenderGraph.EndFrame();
            EndContextRendering(context, cameras);
        }

        private void RenderCamera(ScriptableRenderContext context, Camera camera)
        {
            BeginCameraRendering(context, camera);
            
            if(!PrepareFrameData(context, camera))
                return;
            
            CommandBuffer cmd = CommandBufferPool.Get(camera.name);
            context.SetupCameraProperties(camera);
            
            /*
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
            */
            
            RecordAndExecuteRenderGraph(context, camera, cmd);
            
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
            
            context.Submit();
            
            EndCameraRendering(context, camera);
        }

        private bool PrepareFrameData(ScriptableRenderContext context, Camera camera)
        {
            ScriptableCullingParameters cullingParameters;
            if(!camera.TryGetCullingParameters(out cullingParameters))
                return false;
            CullingResults cullingResults = context.Cull(ref cullingParameters);
            
            CameraData cameraData = m_ContextContainer.GetOrCreate<CameraData>();
            cameraData.camera = camera;
            cameraData.cullingResults = cullingResults;
            
            return true;
        }

        private void RecordAndExecuteRenderGraph(ScriptableRenderContext context, Camera camera, CommandBuffer cmd)
        {
            RenderGraphParameters renderGraphParameters = new RenderGraphParameters()
            {
                executionName = camera.name,
                commandBuffer = cmd,
                scriptableRenderContext = context,
                currentFrameIndex = Time.frameCount,
            };
            m_RenderGraph.BeginRecording(renderGraphParameters);
            
            m_LiteRenderGraphRecorder.RecordRenderGraph(m_RenderGraph,m_ContextContainer);
            
            m_RenderGraph.EndRecordingAndExecute();
        }
    }
}