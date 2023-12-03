using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using ProfilingScope = UnityEngine.Rendering.ProfilingScope;
using System;

namespace HyperToon
{
    [Serializable]
    public class ViewSpaceNormalsTextureSettings
    {
        public RenderTextureFormat colorFormat;
        public int depthBufferBits = 8;
        public Color backgroundColor;
        public FilterMode filterMode;
    }

    // Outlines - material settings
    [Serializable]
    public class OutlineMaterialSettings
    {
        public float outlineScale = 1.5f;
        public float crossMultiplier = 2.5f;
        [Range(0f, 1f)] public float depthThreshold = 1f;
        [Range(0f, 1f)] public float normalThreshold = .4f;
        [Range(0f, 1f)] public float stepAngleThreshold = 1f;
        public float stepAngleMultiplier = 20;
        public Color outlineColor = Color.black;
    }

    // Outlines - full setting
    [Serializable]
    public class OutlinePassSettings
    {
        [SerializeField] public RenderPassEvent OutlinesRenderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        [SerializeField] public LayerMask OutlinesLayerMask;
        [SerializeField] public ViewSpaceNormalsTextureSettings NormalsTextureSettings;
        [SerializeField] public OutlineMaterialSettings OutlineMaterialSettings;
    }
    
    /// <summary>
    /// Normals Pass
    /// </summary>
    public class ViewSpaceNormalsTexturePass : ScriptableRenderPass
    {
        private readonly ViewSpaceNormalsTextureSettings normalsTextureSettings;
        private readonly List<ShaderTagId> shaderTagIdList;
        private readonly RTHandle normals;
        private readonly Material normalsMaterial;
        private FilteringSettings filteringSettings;

        public ViewSpaceNormalsTexturePass(RenderPassEvent renderPassEvent, Material normalsMaterial,
            LayerMask outlinesLayerMask, ViewSpaceNormalsTextureSettings settings)
        {
            shaderTagIdList = new List<ShaderTagId>
            {
                new ("UniversalForward"),
                new ("UniversalForwardOnly"),
                new ("LightweightForward"),
                new ("SRPDefaultUnlit")
            };
            this.renderPassEvent = renderPassEvent;
            normals = RTHandles.Alloc("_SceneViewSpaceNormals", name: "_SceneViewSpaceNormals");
            normalsTextureSettings = settings;
            filteringSettings = new FilteringSettings(RenderQueueRange.opaque, outlinesLayerMask);
            this.normalsMaterial = normalsMaterial;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // descriptor setup
            RenderTextureDescriptor normalsTextureDescriptor = cameraTextureDescriptor;
            normalsTextureDescriptor.colorFormat = normalsTextureSettings.colorFormat;
            normalsTextureDescriptor.depthBufferBits = normalsTextureSettings.depthBufferBits;

            cmd.GetTemporaryRT(Shader.PropertyToID(normals.name), normalsTextureDescriptor,
                normalsTextureSettings.filterMode);
            ConfigureTarget(normals);
            ConfigureClear(ClearFlag.All, normalsTextureSettings.backgroundColor);
        }

        public override void Execute(ScriptableRenderContext ctx, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("SceneViewSpaceNormalsTextureCreation")))
            {
                ctx.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                DrawingSettings drawSettings = CreateDrawingSettings(shaderTagIdList, ref renderingData,
                    renderingData.cameraData.defaultOpaqueSortFlags);
                drawSettings.overrideMaterial = normalsMaterial;
                ctx.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings);
            }

            ctx.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(Shader.PropertyToID(normals.name));
        }
    }

    /// <summary>
    /// Outlines Pass
    /// </summary>
    public class ScreenSpaceOutlinePass : ScriptableRenderPass
    {
        private readonly Material screenSpaceOutlineMaterial;
        private RenderTargetIdentifier cameraColorTarget;
        private RenderTargetIdentifier temporaryBuffer;
        private readonly int temporaryBufferID = Shader.PropertyToID("_TemporaryBuffer");

        public ScreenSpaceOutlinePass(RenderPassEvent renderPassEvent, Material outlineShader)
        {
            this.renderPassEvent = renderPassEvent;
            screenSpaceOutlineMaterial = outlineShader;
        }

        public void Setup(OutlineMaterialSettings settings)
        {
            screenSpaceOutlineMaterial.SetFloat("_OutlineScale", settings.outlineScale);
            screenSpaceOutlineMaterial.SetFloat("_CrossMultiplier", settings.crossMultiplier);
            screenSpaceOutlineMaterial.SetFloat("_DepthThreshold", settings.depthThreshold);
            screenSpaceOutlineMaterial.SetFloat("_NormalThreshold", settings.normalThreshold);
            screenSpaceOutlineMaterial.SetFloat("_StepAngleThreshold", settings.stepAngleThreshold);
            screenSpaceOutlineMaterial.SetFloat("_StepAngleMultiplier", settings.stepAngleMultiplier);
            screenSpaceOutlineMaterial.SetColor("_OutlineColor", settings.outlineColor);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
            temporaryBuffer = temporaryBufferID;
        }

        public override void Execute(ScriptableRenderContext ctx, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("ScreenSpaceOutlines")))
            {
                Blit(cmd, cameraColorTarget, temporaryBuffer);
                Blit(cmd, temporaryBuffer, cameraColorTarget, screenSpaceOutlineMaterial);
            }

            ctx.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(temporaryBufferID);
        }
    }
}
