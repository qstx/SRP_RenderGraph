using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP
{
    [CreateAssetMenu(menuName = "Lite Render Pipline/Lite Render Pipline Asset")]
    public class LiteRPAsset : RenderPipelineAsset<LiteRenderPipeline>
    {
        protected override RenderPipeline CreatePipeline()
        {
            return new LiteRenderPipeline();
        }
    }
}
