using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    private CameraRenderer _cameraRenderer = new CameraRenderer();
    private bool _useGPUInstancing, _useDynamicBatching;

    public CustomRenderPipeline (bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatching)
    {
        // enable SRP batching
        _useDynamicBatching = useDynamicBatching;
        _useGPUInstancing = useGPUInstancing;
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatching;
    }
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            _cameraRenderer.Render(context, camera, _useDynamicBatching, _useGPUInstancing);
        }
    }
}
