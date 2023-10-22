using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Crabs.Rendering
{
    public sealed class DownscaleFeature : ScriptableRendererFeature
    {
        public float pixelsPerUnit = 16.0f;
        
        private Pass pass;
        
        public override void Create()
        {
            pass = new Pass(pixelsPerUnit);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(pass);
        }

        public class Pass : ScriptableRenderPass
        {
            private float ppu;
            
            private Material material;
            
            public Pass(float ppu)
            {
                this.ppu = ppu;
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

                material = new Material(Shader.Find("Hidden/Downscale"));
                material.hideFlags = HideFlags.HideAndDontSave;
                material.name = "Hidden/Downscale";
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var cmd = CommandBufferPool.Get("Downscale Pass");
                
                cmd.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Triangles, 3);
                
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }
        }
    }
}
