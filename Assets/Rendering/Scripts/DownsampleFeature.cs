using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DownsampleFeature : ScriptableRendererFeature
{
    class RenderPass : ScriptableRenderPass
    {

        private string profilingName;
        private Settings _settings;
        private RenderTargetIdentifier sourceID;
        private RenderTargetHandle tempTextureHandle;

        public RenderPass(string profilingName, Settings settings) : base()
        {
            this.profilingName = profilingName;
            _settings = settings;
            tempTextureHandle.Init("_TempBlitMaterialTexture");
        }

        public void SetSource(RenderTargetIdentifier source)
        {
            this.sourceID = source;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilingName);

            RenderTextureDescriptor cameraTextureDesc = renderingData.cameraData.cameraTargetDescriptor;

            if(_settings.downsample)
            {
                float factor = _settings.fixedWidth / (float) cameraTextureDesc.width;
                cameraTextureDesc.width = _settings.fixedWidth; 
                cameraTextureDesc.height = (int) (cameraTextureDesc.height * factor);
            }

            cameraTextureDesc.depthBufferBits = 0;

            cmd.GetTemporaryRT(tempTextureHandle.id, cameraTextureDesc, FilterMode.Point);
            Blit(cmd, sourceID, tempTextureHandle.Identifier());
            Blit(cmd, tempTextureHandle.Identifier(), sourceID);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempTextureHandle.id);
        }
    }

    [System.Serializable]
    public class Settings
    {
        public bool downsample = false;
        public int fixedWidth = 720;
    }

    [SerializeField]
    private Settings settings = new Settings();

    private RenderPass renderPass;

    public override void Create()
    {
        this.renderPass = new RenderPass(name, settings);
        renderPass.renderPassEvent = RenderPassEvent.AfterRenderingPrePasses;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderPass.SetSource(renderer.cameraColorTarget);
        renderer.EnqueuePass(renderPass);
    }
}

