using UnityEngine;
using UnityEngine.Rendering;

public class CameraRenderer
{
    private static ShaderTagId s_unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    private static ShaderTagId[] s_legacyShaderTagIds =
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };
    
    private static Material s_errorMaterial;
    
    private const string BUFFER_NAME = "Render Camera";
    private ScriptableRenderContext _context;
    private Camera _camera;
    private CommandBuffer _commandBuffer = new CommandBuffer{name = BUFFER_NAME};
    private CullingResults _cullingResults;
    
    
    public void Render(ScriptableRenderContext context, Camera camera)
    {
        _context = context;
        _camera = camera;
        
        if (!Cull()) 
        {
            return;
        }

        context.SetupCameraProperties(camera);
        _commandBuffer.ClearRenderTarget(true, true, Color.clear);
        _commandBuffer.BeginSample(BUFFER_NAME);
        {
            ExecuteCommandBuffer();
            DrawVisibleGeometry();
            DrawUnsupportedShaders();
        }
        _commandBuffer.EndSample(BUFFER_NAME);
        ExecuteCommandBuffer();
        
        Submit();
    }
    
    void DrawVisibleGeometry() 
    {
        var sortingSettings = new SortingSettings(_camera){};
        var drawingSettings = new DrawingSettings();
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        
        // set allowed shader pass name
        drawingSettings.SetShaderPassName(0, s_unlitShaderTagId);
        
        // render opaque objects
        drawingSettings.sortingSettings = sortingSettings;
        sortingSettings.criteria = SortingCriteria.CommonOpaque;
        // It has issue here if instantiating FilteringSettings with contructor with empty parameter and assign the value here
        // filteringSettings.renderQueueRange = RenderQueueRange.opaque;
        _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
        
        // render skybox
        _context.DrawSkybox(_camera);
        
        // render transparent objects
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
    }

    void Submit()
    {
        _context.Submit();
    }
    
    void ExecuteCommandBuffer() 
    {
        _context.ExecuteCommandBuffer(_commandBuffer);
        _commandBuffer.Clear();
    }
    
    bool Cull() 
    {
        if (_camera.TryGetCullingParameters(out ScriptableCullingParameters p)) 
        {
            _cullingResults = _context.Cull(ref p);
            return true;
        }
        return false;
    }
    
    void DrawUnsupportedShaders() 
    {
        if (s_errorMaterial == null) 
        {
            s_errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }
        var drawingSettings = new DrawingSettings(s_legacyShaderTagIds[0], new SortingSettings(_camera))
        {
            overrideMaterial = s_errorMaterial
        };
        for (int i = 1; i < s_legacyShaderTagIds.Length; i++) 
        {
            drawingSettings.SetShaderPassName(i, s_legacyShaderTagIds[i]);
        }
        var filteringSettings = FilteringSettings.defaultValue;
        _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
    }
}
