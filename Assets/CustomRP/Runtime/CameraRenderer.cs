using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
	public static readonly ProfilingSampler drawOpaqueObjects = new ProfilingSampler($"{nameof(RenderPipeline)}.DawOpaqueObjects");
	public static readonly ProfilingSampler drawTransparentObjects = new ProfilingSampler($"{nameof(RenderPipeline)}.DrawTransparentObjects");

	private const string BUFFER_NAME = "Render Camera";
	private ScriptableRenderContext _context;
	private Camera _camera;
	private CommandBuffer _commandBuffer = new CommandBuffer {name = BUFFER_NAME};
	private CullingResults _cullingResults;

	private Lighting _lighting = new Lighting();

	public void Render(ScriptableRenderContext context, Camera camera, bool useDynamicBatching, bool useGPUInstancing, ShadowSettings shadowSettings)
	{
		_context = context;
		_camera = camera;

		PrepareBuffer();
		PrepareForSceneWindow();
		CommandBufferSampler.SetupContext(context);

		using (CommandBufferSampler.AddSample(null, BUFFER_NAME))
		{
			if (!Cull(shadowSettings.maxDistance))
			{
				return;
			}

			// using (CommandBufferSampler.AddSample(_commandBuffer, SampleName))
			{
				// the _cullingResults is accessible after culling
				_lighting.Setup(context, _cullingResults, shadowSettings);
			}

			context.SetupCameraProperties(camera);
			
			CameraClearFlags flags = camera.clearFlags;
			_commandBuffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, flags == CameraClearFlags.Color, flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear);
			ExecuteCommandBuffer();
			DrawVisibleGeometry(useDynamicBatching, useGPUInstancing);
			DrawUnsupportedShaders();
			DrawGizmos();

			_lighting.Cleanup();
		}

		Submit();
	}

	void DrawVisibleGeometry(bool useDynamicBatching, bool useGPUInstancing)
	{
		var sortingSettings = new SortingSettings(_camera);
		var drawingSettings = new DrawingSettings();
		var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

		// set allowed shader pass name
		drawingSettings.SetShaderPassName(0, s_unlitShaderTagId);
		drawingSettings.SetShaderPassName(1, s_litShaderTagId);

		// set batching 
		drawingSettings.enableDynamicBatching = useDynamicBatching;
		drawingSettings.enableInstancing = useGPUInstancing;

		//GI data
		drawingSettings.perObjectData = PerObjectData.Lightmaps | PerObjectData.ShadowMask | PerObjectData.LightProbe | PerObjectData.OcclusionProbe | PerObjectData.LightProbeProxyVolume | PerObjectData.OcclusionProbeProxyVolume;

		using (CommandBufferSampler.AddSample(null, "RenderOpaqueObjects"))
		{
			// render opaque objects
			sortingSettings.criteria = SortingCriteria.CommonOpaque;
			drawingSettings.sortingSettings = sortingSettings;
			// It has issue here if instantiating FilteringSettings with contructor with empty parameter and assign the value here
			// filteringSettings.renderQueueRange = RenderQueueRange.opaque;
			_context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
		}

		// render skybox
		_context.DrawSkybox(_camera);

		using (CommandBufferSampler.AddSample(null, "RenderTransparentObjects"))
		{
			// render transparent objects
			sortingSettings.criteria = SortingCriteria.CommonTransparent;
			drawingSettings.sortingSettings = sortingSettings;
			filteringSettings.renderQueueRange = RenderQueueRange.transparent;
			_context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
		}
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

	bool Cull(float maxShadowDistance)
	{
		if (_camera.TryGetCullingParameters(out ScriptableCullingParameters p))
		{
			p.shadowDistance = Mathf.Min(maxShadowDistance, _camera.farClipPlane);
			_cullingResults = _context.Cull(ref p);
			return true;
		}

		return false;
	}
}