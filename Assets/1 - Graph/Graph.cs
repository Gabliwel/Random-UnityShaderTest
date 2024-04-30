using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://catlikecoding.com/unity/tutorials/basics/building-a-graph/

public class Graph : MonoBehaviour
{
	[SerializeField] private Transform pointPrefab;
	[SerializeField, Range(10, 100)] private int resolution = 10;
	
	private Transform[] points;

	void Awake()
	{
		float step = 2f / resolution;
		var position = Vector3.zero;
		var scale = Vector3.one * step;

		points = new Transform[resolution];

		for (int i = 0; i < points.Length; i++)
		{
			Transform point = points[i] = Instantiate(pointPrefab);
			position.x = (i + 0.5f) * step - 1f;
			point.localPosition = position;
			point.localScale = scale;
		}

	}

	void Update()
	{
		float time = Time.time;
		for (int i = 0; i < points.Length; i++) {
			Transform point = points[i];
			Vector3 position = point.localPosition;
			position.y = Mathf.Sin(Mathf.PI * (position.x + Time.time));
			point.localPosition = position;
		}
	}
}
