using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HyperToon
{
    [Serializable]
    public class TransparencyPassSettings
    {
        public string TextureName = "_GrabPassTransparent";
        public LayerMask LayerMask;
    }
    
    /// <summary>
    /// Transparency grab pass - grabs the screen color and transparency
    /// </summary>
    public class TransparencyGrabPass : ScriptableRenderPass
    {
        private readonly TransparencyPassSettings settings;
        private readonly RTHandle tempColorTarget;
        private RenderTargetIdentifier cameraTarget;

        public TransparencyGrabPass(TransparencyPassSettings settings)
        {
            this.settings = settings;
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            tempColorTarget = RTHandles.Alloc(settings.TextureName, name: settings.TextureName);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(Shader.PropertyToID(tempColorTarget.name), cameraTextureDescriptor);
            cmd.SetGlobalTexture(settings.TextureName, tempColorTarget);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            cameraTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
            CommandBuffer cmd = CommandBufferPool.Get();
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            Blit(cmd, cameraTarget, tempColorTarget);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(Shader.PropertyToID(tempColorTarget.name));
        }
    }

    /// <summary>
    /// Transparency render pass - renders the transparency to a texture
    /// </summary>
    public class TransparencyRenderPass : ScriptableRenderPass
    {
        private readonly List<ShaderTagId> shaderTagIdList;
        private FilteringSettings m_FilteringSettings;
        private RenderStateBlock m_RenderStateBlock;

        public TransparencyRenderPass(TransparencyPassSettings settings)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents + 1;
            shaderTagIdList = new List<ShaderTagId>
            {
                new ("UniversalForward"),
                new ("UniversalForwardOnly"),
                new ("LightweightForward"),
                new ("SRPDefaultUnlit")
            };
            m_FilteringSettings = new FilteringSettings(RenderQueueRange.all, settings.LayerMask);
            m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            DrawingSettings drawSettings = CreateDrawingSettings(shaderTagIdList, ref renderingData,
                SortingCriteria.CommonTransparent);
            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref m_FilteringSettings,
                ref m_RenderStateBlock);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}