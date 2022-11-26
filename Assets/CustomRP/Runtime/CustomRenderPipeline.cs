using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    private CameraRenderer _cameraRenderer = new CameraRenderer();
    private bool _useGPUInstancing, _useDynamicBatching;
    private ShadowSettings _shadowSettings;
    
    public static readonly ProfilingSampler beginContextRendering  = new ProfilingSampler($"{nameof(RenderPipeline)}.{nameof(BeginContextRendering)}");
    public static readonly ProfilingSampler endContextRendering    = new ProfilingSampler($"{nameof(RenderPipeline)}.{nameof(EndContextRendering)}");

    public CustomRenderPipeline (bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatching, ShadowSettings shadowSettings)
    {
        // enable SRP batching
        _useDynamicBatching = useDynamicBatching;
        _useGPUInstancing = useGPUInstancing;
        _shadowSettings = shadowSettings;
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatching;
        GraphicsSettings.lightsUseLinearIntensity = true;

        RenderPipelineManager.beginFrameRendering += (context, cameras) =>
        {
            // Debug.Log("RenderPipelineManager.beginFrameRendering");
        };
        
        RenderPipelineManager.endFrameRendering += (context, cameras) =>
        {
            // Debug.Log("RenderPipelineManager.endFrameRendering");
        };
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        Render(context, new List<Camera>(cameras));
    }

    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        foreach (var camera in cameras)
        {
            _cameraRenderer.Render(context, camera, _useDynamicBatching, _useGPUInstancing, _shadowSettings);
        }

    }
}
