using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CommandBufferSampler : IDisposable
{
	private static ScriptableRenderContext _context;
	private readonly CommandBuffer _commandBuffer;
	private readonly string _sampleName;

	#region STATIC_METHODS
	public static void SetupContext(ScriptableRenderContext context)
	{
		_context = context;
	}
	#endregion

	#region CONSTRUCTOR
	private CommandBufferSampler(CommandBuffer commandBuffer, string sampleName)
	{
		_commandBuffer = commandBuffer;
		_sampleName = sampleName;
	}
	#endregion

	private void BeginSample()
	{
		_commandBuffer.BeginSample(_sampleName);
		_context.ExecuteCommandBuffer(_commandBuffer);
		_commandBuffer.Clear();
	}

	private void EndSample()
	{
		_commandBuffer.EndSample(_sampleName);
		_context.ExecuteCommandBuffer(_commandBuffer);
		_commandBuffer.Clear();
	}

	public static CommandBufferSampler AddSample(CommandBuffer commandBuffer, string sampleName)
	{
		CommandBufferSampler commandBufferSampler = new CommandBufferSampler(commandBuffer, sampleName);
		commandBufferSampler.BeginSample();
		return commandBufferSampler;
	}

	public void Dispose()
	{
		EndSample();
	}
}