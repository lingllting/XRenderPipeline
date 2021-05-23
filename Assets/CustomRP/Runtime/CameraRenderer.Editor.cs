using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
#if UNITY_EDITOR
    string SampleName { get; set; }
    partial void DrawGizmos();
    partial void DrawUnsupportedShaders();
    partial void PrepareForSceneWindow();
    partial void PrepareBuffer();
    
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

    partial void DrawUnsupportedShaders() 
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
    
    partial void DrawGizmos()
    {
        if (Handles.ShouldRenderGizmos()) 
        {
            _context.DrawGizmos(_camera, GizmoSubset.PreImageEffects);
            _context.DrawGizmos(_camera, GizmoSubset.PostImageEffects);
        }
    }
    
    partial void PrepareForSceneWindow()
    {
        if (_camera.cameraType == CameraType.SceneView) 
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
        }
    }
    
    partial void PrepareBuffer () 
    {
        Profiler.BeginSample("Editor Only");
        _commandBuffer.name = SampleName = _camera.name;
        Profiler.EndSample();
    }
#else
    const string SampleName = BUFFER_NAME;
#endif
}
