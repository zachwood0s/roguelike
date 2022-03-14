//    Copyright (C) 2020 Ned Makes Games

//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU General Public License for more details.

//    You should have received a copy of the GNU General Public License
//    along with this program. If not, see <https://www.gnu.org/licenses/>.

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlitMaterialFeature : ScriptableRendererFeature
{
    class RenderPass : ScriptableRenderPass
    {
        private string _profileName;
        private Settings _settings;
        private RenderTargetIdentifier sourceID;
        private RenderTargetHandle tempTextureHandle;
        private RenderTargetHandle tempDownsampleHandle;

        public RenderPass(string profilingName, Settings settings) : base()
        {
            _profileName = profilingName;
            _settings = settings;
            tempTextureHandle.Init("_TempBlitMaterialTexture");
            tempDownsampleHandle.Init("_TempDownsampleMaterialTexture");
        }

        public void SetSource(RenderTargetIdentifier source)
        {
            this.sourceID = source;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(_profileName);

            RenderTextureDescriptor cameraTextureDesc = renderingData.cameraData.cameraTargetDescriptor;
            FilterMode mode = FilterMode.Bilinear;

            if(_settings.downsample)
            {
                float factor = _settings.fixedWidth / (float) cameraTextureDesc.width;
                cameraTextureDesc.width = _settings.fixedWidth; 
                cameraTextureDesc.height = (int) (cameraTextureDesc.height * factor);
                mode = FilterMode.Point;
            }
            cameraTextureDesc.depthBufferBits = 0;

            cmd.GetTemporaryRT(tempTextureHandle.id, cameraTextureDesc, mode);
            // Render to a downsampled texture so the materials apply correctly.
            // For some reason the target scaling was not being sent to the material and I was too lazy to 
            // figure out why.
            cmd.GetTemporaryRT(tempDownsampleHandle.id, cameraTextureDesc, mode);
            Blit(cmd, sourceID, tempDownsampleHandle.Identifier());
            Blit(cmd, tempDownsampleHandle.Identifier(), tempTextureHandle.Identifier(), _settings.material, _settings.materialPassIndex);
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
        public Material material;
        public int materialPassIndex = -1; // -1 means render all passes
        public RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    [SerializeField]
    private Settings settings = new Settings();

    private RenderPass renderPass;

    public Material Material
    {
        get => settings.material;
    }

    public override void Create()
    {
        this.renderPass = new RenderPass(name, settings);
        renderPass.renderPassEvent = settings.renderEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderPass.SetSource(renderer.cameraColorTarget);
        renderer.EnqueuePass(renderPass);
    }
}

