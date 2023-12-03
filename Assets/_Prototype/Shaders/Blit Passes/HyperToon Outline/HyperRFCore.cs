using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HyperToon
{
    public class HyperRFCore : ScriptableRendererFeature
    {
        // Common
        
        [Header("Transparency")]
        // Transparency passes
        [SerializeField] private TransparencyPassSettings transparencyPassSettings;
        private TransparencyGrabPass grabPass;
        private TransparencyRenderPass renderPass;
        
        [Header("Outline")]
        // Outlines passes
        [SerializeField] private OutlinePassSettings outlinePassSettings;
        private Material normalsMaterial;
        private Material outlinesMaterial;
        private ViewSpaceNormalsTexturePass viewSpaceNormalsTexturePass;
        private ScreenSpaceOutlinePass screenSpaceOutlinePass;

        [Header("Lightweight Blit Passes Stack")]
        // Lightweight Blit Passes
        [SerializeField]
        private BlitPassSettings[] blitPassSettingsStack = new [] {new BlitPassSettings()};
        private BlitPass[] blitPasses;
        
        // Complete render feature
        public override void Create()
        {
            // transparency passes
            grabPass = new TransparencyGrabPass(transparencyPassSettings);
            renderPass = new TransparencyRenderPass(transparencyPassSettings);
            
            // outlines passes
            // create materials
            normalsMaterial = CoreUtils.CreateEngineMaterial("HyperToon/RenderFeatures/Hidden/HyperToon_ViewSpaceNormalsShader");
            outlinesMaterial = CoreUtils.CreateEngineMaterial("HyperToon/RenderFeatures/Hidden/HyperToon_OutlineShader");
            // create passes
            viewSpaceNormalsTexturePass = new ViewSpaceNormalsTexturePass(outlinePassSettings.OutlinesRenderPassEvent, normalsMaterial, 
                outlinePassSettings.OutlinesLayerMask, outlinePassSettings.NormalsTextureSettings);
            screenSpaceOutlinePass = new ScreenSpaceOutlinePass(outlinePassSettings.OutlinesRenderPassEvent, outlinesMaterial);
            
            // blit passes
            blitPasses = new BlitPass[blitPassSettingsStack.Length];
            for (int i = 0; i < blitPassSettingsStack.Length; i++)
            {
                blitPasses[i] = new BlitPass(blitPassSettingsStack[i], name);
            }
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // blit passes added
            foreach (var blitPass in blitPasses)
            {
                if (!blitPass.settings.Activate) continue;
                blitPass.Setup();
                renderer.EnqueuePass(blitPass);
            }
            // transparency passes added
            renderer.EnqueuePass(grabPass);
            renderer.EnqueuePass(renderPass);
            // outlines passes added
            screenSpaceOutlinePass.Setup(outlinePassSettings.OutlineMaterialSettings);
            renderer.EnqueuePass(viewSpaceNormalsTexturePass);
            renderer.EnqueuePass(screenSpaceOutlinePass);
        }
    }
}
