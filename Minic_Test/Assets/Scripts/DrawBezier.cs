using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using S = System;

namespace DebugTests
{
	[DisallowMultipleComponent]
	public class DrawBezier : MonoBehaviour
	{
		#region	Private variables
		[SerializeField]
		private int subdivisions = 15;
		#endregion
		#region Private properties
		private Vector3[] points =>
			transform
			.GetComponentsInChildren<Transform>()
			.Where<Transform>(t => t != transform)
			.Select<Transform, Vector3>(t => t.position)
			.ToArray();
		#endregion
		#region	Lifecycle
		void OnDrawGizmos()
		{
			Vector3[] points = this.points;

			if(points.Length > 1 )
			{
				for(int i = 1; i <= subdivisions; i++)
				{
					Gizmos.DrawLine(
						Bezier3D((i - 1) / (float)subdivisions, points),
						Bezier3D(i / (float)subdivisions, points)
					);
				}
			}
		}
		#endregion
		#region Private static methods
		private int Fact(int n)
		{
			if(n < 2)
				return 1;
			return n * Fact(n - 1);
		}
		private int BK(int n, int k) => Fact(n) / (Fact(k) * Fact(n - k));
		private float Bezier(float t, params float[] points)
		{
			float bezier = 0.0f;
			int n = points.Length - 1;

			for(int i = 0; i <= n; i++)
				bezier +=
					BK(n, i) *
					points[i] *
					Mathf.Pow(1 - t, n - i) *
					Mathf.Pow(t, i);

			return bezier;
		}
		private Vector3 Bezier3D(float t, params Vector3[] points)
		{
			return new Vector3(
				Bezier(t, points.Select<Vector3, float>(v => v.x).ToArray()),
				Bezier(t, points.Select<Vector3, float>(v => v.y).ToArray()),
				Bezier(t, points.Select<Vector3, float>(v => v.z).ToArray())
			);
		}
		#endregion
	}
}
