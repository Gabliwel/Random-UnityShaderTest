using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://catlikecoding.com/unity/tutorials/basics/building-a-graph/
// https://catlikecoding.com/unity/tutorials/basics/mathematical-surfaces/

public class GraphWithLibrary : MonoBehaviour
{
	public enum TransitionMode { Cycle, Random }

	[SerializeField] private Transform pointPrefab;
	[SerializeField, Range(10, 200)] private int resolution = 10;
	[SerializeField] private FunctionLibrary.FunctionName function;
	[SerializeField, Min(0f)] private float functionDuration = 1f, transitionDuration = 1f;
	[SerializeField] private TransitionMode transitionMode;

	private Transform[] points;
	private FunctionLibrary.Function f;
	private float duration;

	bool transitioning;
	FunctionLibrary.FunctionName transitionFunction;

	void Awake()
	{
		float step = 2f / resolution;
		var scale = Vector3.one * step;

		points = new Transform[resolution * resolution];

		for (int i = 0; i < points.Length; i++)
		{
			Transform point = points[i] = Instantiate(pointPrefab);
			point.localScale = scale;
		}
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

		if (transitioning)
		{
			UpdateFunctionTransition();
		}
		else
		{
			UpdateFunction();
		}
	}

	void PickNextFunction()
	{
		function = transitionMode == TransitionMode.Cycle ?
			FunctionLibrary.GetNextFunctionName(function) :
			FunctionLibrary.GetRandomFunctionNameOtherThan(function);
	}

	void UpdateFunction()
	{
		f = FunctionLibrary.GetFunction(function);

		float time = Time.time;
		float step = 2f / resolution;
		float v = 0.5f * step - 1f;
		for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
		{
			if (x == resolution)
			{
				x = 0;
				z += 1;
				v = (z + 0.5f) * step - 1f;
			}
			float u = (x + 0.5f) * step - 1f;
			points[i].localPosition = f(u, v, time);
		}
	}

	void UpdateFunctionTransition()
	{
		FunctionLibrary.Function
			from = FunctionLibrary.GetFunction(transitionFunction),
			to = FunctionLibrary.GetFunction(function);
		float progress = duration / transitionDuration;

		float time = Time.time;
		float step = 2f / resolution;
		float v = 0.5f * step - 1f;
		for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
		{
			if (x == resolution)
			{
				x = 0;
				z += 1;
				v = (z + 0.5f) * step - 1f;
			}
			float u = (x + 0.5f) * step - 1f;
			points[i].localPosition = FunctionLibrary.Morph(
				u, v, time, from, to, progress
			);
		}
	}
}
