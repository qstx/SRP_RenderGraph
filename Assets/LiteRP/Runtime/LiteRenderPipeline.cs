using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP
{
    public class LiteRenderPipeline : RenderPipeline
    {
        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            base.Render(context, cameras);
        }
    }
}