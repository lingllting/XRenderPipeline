using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    private CameraRenderer _cameraRenderer = new CameraRenderer();
    private bool _useGPUInstancing, _useDynamicBatching;
    private ShadowSettings _shadowSettings;

    public CustomRenderPipeline (bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatching, ShadowSettings shadowSettings)
    {
        // enable SRP batching
        _useDynamicBatching = useDynamicBatching;
        _useGPUInstancing = useGPUInstancing;
        _shadowSettings = shadowSettings;
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatching;
        GraphicsSettings.lightsUseLinearIntensity = true;
    }
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            _cameraRenderer.Render(context, camera, _useDynamicBatching, _useGPUInstancing, _shadowSettings);
        }
    }
}
