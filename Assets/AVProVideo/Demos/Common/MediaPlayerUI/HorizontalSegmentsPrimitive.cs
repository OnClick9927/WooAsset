// UnityEngine.UI was moved to a package in 2019.2.0
// Unfortunately no way to test for this across all Unity versions yet
// You can set up the asmdef to reference the new package, but the package doesn't 
// existing in Unity 2017 etc, and it throws an error due to missing reference
#define AVPRO_PACKAGE_UNITYUI
#if (UNITY_2019_2_OR_NEWER && AVPRO_PACKAGE_UNITYUI) || (!UNITY_2019_2_OR_NEWER)

using UnityEngine;
using UnityEngine.UI;

namespace RenderHeads.Media.AVProVideo.Demos.UI
{
	/// Fill a rectangle region with horizontal segments along it
	[ExecuteInEditMode]
	public class HorizontalSegmentsPrimitive : Graphic
	{
		private float[] _segments = { 0f, 0f };
		public float[] Segments { get { return _segments; } set { SetSegments(value); } }

		private void SetSegments(float[] segments)
		{
			if (segments != null && segments.Length > 1)
			{
				_segments = segments;
			}
			else
			{
				_segments = new float[] { 0f, 0f };
			}

			// TODO: detect whether a change actually occured before setting to dirty
			SetVerticesDirty();
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			Vector2 corner1 = Vector2.zero;
			Vector2 corner2 = Vector2.zero;

			corner1.x = 0f;
			corner1.y = 0f;
			corner2.x = 1f;
			corner2.y = 1f;

			corner1.x -= rectTransform.pivot.x;
			corner1.y -= rectTransform.pivot.y;
			corner2.x -= rectTransform.pivot.x;
			corner2.y -= rectTransform.pivot.y;

			corner1.x *= rectTransform.rect.width;
			corner1.y *= rectTransform.rect.height;
			corner2.x *= rectTransform.rect.width;
			corner2.y *= rectTransform.rect.height;

			vh.Clear();

			int numQuads = _segments.Length / 2;

			UIVertex vert = UIVertex.simpleVert;
			int vi = 0;
			for (int i = 0; i < numQuads; i++)
			{
				float x1 = _segments[i * 2 + 0] * (corner2.x - corner1.x) + corner1.x;
				float x2 = _segments[i * 2 + 1] * (corner2.x - corner1.x) + corner1.x;

				vert.position = new Vector2(x1, corner1.y);
				vert.color = color;
				vh.AddVert(vert);

				vert.position = new Vector2(x1, corner2.y);
				vert.color = color;
				vh.AddVert(vert);

				vert.position = new Vector2(x2, corner2.y);
				vert.color = color;
				vh.AddVert(vert);

				vert.position = new Vector2(x2, corner1.y);
				vert.color = color;
				vh.AddVert(vert);

				vh.AddTriangle(0 + vi, 1 + vi, 2 + vi);
				vh.AddTriangle(2 + vi, 3 + vi, 0 + vi);
				vi += 4;
			}
		}
	}
}
#endif