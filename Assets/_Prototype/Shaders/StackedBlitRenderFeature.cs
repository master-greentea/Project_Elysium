using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class StackedBlitRenderFeature : ScriptableRendererFeature
{
	// Lightweight Blit Passes
	[SerializeField]
	private BlitPassSettings[] blitPassSettingsStack = new [] {new BlitPassSettings()};
	private BlitPass[] blitPasses;
	
	public override void Create()
    {
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
    }
}


[Serializable]
public class BlitPassSettings
{
	public bool Activate = true;
	public RenderPassEvent BlitRenderPassEvent = RenderPassEvent.AfterRenderingOpaques;
	public Material BlitMaterial;
}

public class BlitPass : ScriptableRenderPass
{
	public BlitPassSettings settings;
	private readonly Material blitMaterial;
	private FilterMode filterMode;
	private RenderTargetIdentifier source;
	private RenderTargetIdentifier destination;
	private readonly RTHandle temporaryColorTexture;
	private readonly RTHandle destinationTexture;
	private readonly string profilerTag;

	public BlitPass(BlitPassSettings settings, string tag)
	{
		this.settings = settings;
		renderPassEvent = settings.BlitRenderPassEvent;
		blitMaterial = settings.BlitMaterial;
		profilerTag = tag;
		temporaryColorTexture = RTHandles.Alloc("_TemporaryColorTexture", name: "_TemporaryColorTexture");
		destinationTexture = RTHandles.Alloc("_DestinationTexture", name: "_DestinationTexture");
	}

	public void Setup() {
		ConfigureInput(ScriptableRenderPassInput.Normal);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
		CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
		RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
		opaqueDesc.depthBufferBits = 0;

		// Set Source / Destination
		var renderer = renderingData.cameraData.renderer;
		if (renderingData.cameraData.camera.cameraType == CameraType.Reflection) return;

		// note : Seems this has to be done in here rather than in AddRenderPasses to work correctly in 2021.2+
		source = renderer.cameraColorTargetHandle;
		destination = renderer.cameraColorTargetHandle;
		
		cmd.GetTemporaryRT(Shader.PropertyToID(destinationTexture.name), opaqueDesc, filterMode);
		
		if (source == destination) {
			cmd.GetTemporaryRT(Shader.PropertyToID(temporaryColorTexture.name), opaqueDesc, filterMode);
			Blit(cmd, source, temporaryColorTexture.nameID, blitMaterial);
			Blit(cmd, temporaryColorTexture.nameID, destination);
		} else {
			Blit(cmd, source, destination, blitMaterial);
		}

		context.ExecuteCommandBuffer(cmd);
		CommandBufferPool.Release(cmd);
	}

	public override void FrameCleanup(CommandBuffer cmd) {
		cmd.ReleaseTemporaryRT(Shader.PropertyToID(destinationTexture.name));
		if (source == destination) {
			cmd.ReleaseTemporaryRT(Shader.PropertyToID(temporaryColorTexture.name));
		}
	}
}
