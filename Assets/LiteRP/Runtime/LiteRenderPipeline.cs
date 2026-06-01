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
            RTHandles.Initialize(Screen.width, Screen.height);
            m_RenderGraph = new RenderGraph("LiteRPRenderGraph");
            m_RenderGraph.nativeRenderPassesEnabled = LiteRPUtils.CanNativeRenderPassesEnabled();
            m_LiteRenderGraphRecorder = new LiteRenderGraphRecorder();
            m_ContextContainer = new ContextContainer();
        }

        private void CleanupRenderGraph()
        {
            m_ContextContainer?.Dispose();
            m_ContextContainer = null;
            m_LiteRenderGraphRecorder?.Dispose();
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
            
            RecordAndExecuteRenderGraph(context, camera, cmd);
            
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
            
            context.Submit();
            
            EndCameraRendering(context, camera);
        }

        private bool PrepareFrameData(ScriptableRenderContext context, Camera camera)
        {
            if(!camera.TryGetCullingParameters(out var cullingParameters))
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
                executionId = camera.GetEntityId(),
                commandBuffer = cmd,
                scriptableRenderContext = context,
                currentFrameIndex = Time.frameCount,
                generateDebugData = camera.cameraType != CameraType.Preview && !camera.isProcessingRenderRequest,
            };
            m_RenderGraph.BeginRecording(renderGraphParameters);
            
            m_LiteRenderGraphRecorder.RecordRenderGraph(m_RenderGraph,m_ContextContainer);
            
            m_RenderGraph.EndRecordingAndExecute();
        }
    }
}