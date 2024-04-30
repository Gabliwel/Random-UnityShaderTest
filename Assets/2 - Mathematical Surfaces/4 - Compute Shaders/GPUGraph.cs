using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://catlikecoding.com/unity/tutorials/basics/compute-shaders/

public class GPUGraph : MonoBehaviour
{
	public enum TransitionMode { Cycle, Random }

	private const int maxResolution = 1000;

	[SerializeField, Range(10, maxResolution)] private int resolution = 10;
	[SerializeField] private FunctionLibrary.FunctionName function;
	[SerializeField, Min(0f)] private float functionDuration = 1f, transitionDuration = 1f;
	[SerializeField] private TransitionMode transitionMode;

	private FunctionLibrary.Function f;
	private float duration;

	private bool transitioning;
	private FunctionLibrary.FunctionName transitionFunction;

	// change for perf
	[SerializeField] private Material material;
	[SerializeField] private Mesh mesh;

	[SerializeField] private ComputeShader computeShader;
	private ComputeBuffer positionsBuffer;

	static readonly int
		positionsId = Shader.PropertyToID("_Positions"),
		resolutionId = Shader.PropertyToID("_Resolution"),
		stepId = Shader.PropertyToID("_Step"),
		timeId = Shader.PropertyToID("_Time"),
		transitionProgressId = Shader.PropertyToID("_TransitionProgress");

	void Awake()
	{
		// last argument is a 3d vector position size, so three times four bytes
		positionsBuffer = new ComputeBuffer(resolution * resolution, 3 * 4);
	}

	void OnEnable()
	{
		positionsBuffer = new ComputeBuffer(maxResolution * maxResolution, 3 * 4);
	}

	void OnDisable()
	{
		positionsBuffer.Release();
		positionsBuffer = null;
	}

	void Update()
	{
		// to cycle the differents anims
		duration += Time.deltaTime;

		if (transitioning && duration >= transitionDuration)
		{
			duration -= transitionDuration;
			transitioning = false;
		}
		else if (duration >= functionDuration)
		{
			duration -= functionDuration;
			transitioning = true;
			transitionFunction = function;
			PickNextFunction();
		}
		UpdateFunctionOnGPU();
	}

	void PickNextFunction()
	{
		function = transitionMode == TransitionMode.Cycle ?
			FunctionLibrary.GetNextFunctionName(function) :
			FunctionLibrary.GetRandomFunctionNameOtherThan(function);
	}

	void UpdateFunctionOnGPU()
	{
		float step = 2f / resolution;
		computeShader.SetInt(resolutionId, resolution);
		computeShader.SetFloat(stepId, step);
		computeShader.SetFloat(timeId, Time.time);

		if (transitioning)
		{
			computeShader.SetFloat(
				transitionProgressId,
				Mathf.SmoothStep(0f, 1f, duration / transitionDuration)
			);
		}

		var kernelIndex = (int)function + (int)(transitioning ? transitionFunction : function) * 5;
		computeShader.SetBuffer(kernelIndex, positionsId, positionsBuffer);

		int groups = Mathf.CeilToInt(resolution / 8f);
		computeShader.Dispatch(kernelIndex, groups, groups, 1);

		material.SetBuffer(positionsId, positionsBuffer);
		material.SetFloat(stepId, step);

		var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
		Graphics.DrawMeshInstancedProcedural(
			mesh, 0, material, bounds, resolution * resolution
		);
	}
}
