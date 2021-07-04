using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    private CameraRenderer _cameraRenderer = new CameraRenderer();
    
    public CustomRenderPipeline ()
    {
        // enable SRP batching
        GraphicsSettings.useScriptableRenderPipelineBatching = false;
    }
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            _cameraRenderer.Render(context, camera);
        }
    }
}
