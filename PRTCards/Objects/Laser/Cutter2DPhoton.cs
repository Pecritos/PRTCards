using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Cutter2DPhoton
{
	[System.Serializable]
	public struct CutterPieceData
	{
		public Vector2[] localPoints;
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 scale;
		public int layer;
		public Color color;
		public int sortingLayerID;
		public int sortingOrder;
		public bool withPhysics;
		public int spriteID;
		public float mass;
		public float gravityScale;
	}

	private static Dictionary<Sprite, int> spriteToID = new Dictionary<Sprite, int>();
	private static Dictionary<int, Sprite> idToSprite = new Dictionary<int, Sprite>();
	private static int nextID = 0;

	public static int GetSpriteID(Sprite sprite)
	{
		if (sprite == null) return -1;
		if (!spriteToID.TryGetValue(sprite, out int id))
		{
			id = nextID++;
			spriteToID[sprite] = id;
			idToSprite[id] = sprite;
		}
		return id;
	}

	public static Sprite GetSpriteByID(int id)
	{
		if (idToSprite.TryGetValue(id, out Sprite sprite)) return sprite;
		return null;
	}

	public static List<CutterPieceData> CutAndReturnData(GameObject original, Vector2 pointA, Vector2 pointB)
	{
		var col = original.GetComponent<PolygonCollider2D>();
		if (col == null) return null;

		List<Vector2> worldPoints = col.points.Select(p => (Vector2)original.transform.TransformPoint(p)).ToList();
		List<Vector2> side1 = new List<Vector2>();
		List<Vector2> side2 = new List<Vector2>();

		for (int i = 0; i < worldPoints.Count; i++)
		{
			Vector2 P = worldPoints[i];
			Vector2 Q = worldPoints[(i + 1) % worldPoints.Count];
			bool aboveP = IsAbove(P, pointA, pointB);
			bool aboveQ = IsAbove(Q, pointA, pointB);

			if (aboveP) side1.Add(P); else side2.Add(P);

			if (aboveP != aboveQ && SegmentIntersect(P, Q, pointA, pointB, out Vector2 hit))
			{
				side1.Add(hit);
				side2.Add(hit);
			}
		}

		List<CutterPieceData> result = new List<CutterPieceData>();

		Sprite finalSprite = null;
		Color finalColor = Color.white;
		int finalSortingLayer = 0;
		int finalSortingOrder = 0;

		Transform colorChild = original.transform.Find("Color");
		SpriteRenderer srColor = (colorChild != null) ? colorChild.GetComponent<SpriteRenderer>() : null;

		if (srColor != null)
		{
			finalSprite = srColor.sprite;
			finalColor = srColor.color;
			finalSortingLayer = srColor.sortingLayerID;
			finalSortingOrder = srColor.sortingOrder;
		}

		if (finalSprite == null)
		{
			MeshRenderer mr = original.GetComponent<MeshRenderer>();
			if (mr != null && mr.material != null)
			{
				var pd = original.GetComponent<PieceData>();
				if (pd != null) finalSprite = pd.sprite;

				finalColor = mr.material.color;
				finalSortingLayer = mr.sortingLayerID;
				finalSortingOrder = mr.sortingOrder;
			}
		}

		if (finalSprite == null)
		{
			SpriteRenderer srRoot = original.GetComponent<SpriteRenderer>();
			if (srRoot != null)
			{
				finalSprite = srRoot.sprite;
				finalColor = srRoot.color;
				finalSortingLayer = srRoot.sortingLayerID;
				finalSortingOrder = srRoot.sortingOrder;
			}
		}

		if (finalSprite == null)
		{
			var pd = original.GetComponent<PieceData>();
			if (pd != null) finalSprite = pd.sprite;
		}

		void AddPiece(List<Vector2> worldPts)
		{
			if (worldPts.Count < 3) return;

			Vector2[] localPts = worldPts.Select(p => (Vector2)original.transform.InverseTransformPoint(p)).ToArray();

			result.Add(new CutterPieceData
			{
				spriteID = GetSpriteID(finalSprite),
				localPoints = localPts,
				position = original.transform.position,
				rotation = original.transform.rotation,
				scale = original.transform.localScale,
				layer = original.layer,
				color = finalColor,
				sortingLayerID = finalSortingLayer,
				sortingOrder = finalSortingOrder,
				withPhysics = original.GetComponent<Rigidbody2D>() != null,
				mass = original.GetComponent<Rigidbody2D>()?.mass ?? 20000f,
				gravityScale = original.GetComponent<Rigidbody2D>()?.gravityScale ?? 1f
			});
		}

		AddPiece(side1);
		AddPiece(side2);
		return result;
	}

	private static bool IsAbove(Vector2 p, Vector2 a, Vector2 b) => ((b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x)) > 0;

	private static bool SegmentIntersect(Vector2 p, Vector2 q, Vector2 a, Vector2 b, out Vector2 r)
	{
		Vector2 v = q - p, w = b - a;
		float d = v.x * w.y - v.y * w.x;
		if (Mathf.Approximately(d, 0f)) { r = default; return false; }
		float t = ((a - p).x * w.y - (a - p).y * w.x) / d;
		r = p + t * v;
		return t >= 0f && t <= 1f;
	}

	public static Mesh CreateMeshFromPolygon(List<Vector2> pts, Sprite sprite)
	{
		Mesh mesh = new Mesh();
		Vector3[] verts = pts.Select(p => (Vector3)p).ToArray();
		int[] triangles = new Triangulator(pts.ToArray()).Triangulate();

		for (int i = 0; i < triangles.Length; i += 3)
		{
			int temp = triangles[i + 1];
			triangles[i + 1] = triangles[i + 2];
			triangles[i + 2] = temp;
		}

		mesh.vertices = verts;
		mesh.triangles = triangles;

		Vector2[] uvs = new Vector2[pts.Count];
		if (sprite != null)
		{
			Rect rect = sprite.rect;
			Vector2 pivot = sprite.pivot;
			float ppu = sprite.pixelsPerUnit;
			Texture2D tex = sprite.texture;

			for (int i = 0; i < pts.Count; i++)
			{
				float px = verts[i].x * ppu + pivot.x;
				float py = verts[i].y * ppu + pivot.y;
				uvs[i] = new Vector2((rect.x + px) / tex.width, (rect.y + py) / tex.height);
			}
		}
		else
		{
			float minX = pts.Min(p => p.x), maxX = pts.Max(p => p.x), minY = pts.Min(p => p.y), maxY = pts.Max(p => p.y);
			for (int i = 0; i < pts.Count; i++) uvs[i] = new Vector2((pts[i].x - minX) / (maxX - minX), (pts[i].y - minY) / (maxY - minY));
		}

		mesh.uv = uvs;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		return mesh;
	}
}

public class Triangulator
{
	List<Vector2> m_pts;
	public Triangulator(Vector2[] pts) { m_pts = new List<Vector2>(pts); }

	public int[] Triangulate()
	{
		var indices = new List<int>();
		int n = m_pts.Count;
		int[] V = new int[n];
		if (Area() > 0) for (int v = 0; v < n; v++) V[v] = v;
		else for (int v = 0; v < n; v++) V[v] = (n - 1) - v;

		int nv = n, count = 2 * nv;
		for (int v = nv - 1; nv > 2;)
		{
			if ((count--) <= 0) break;
			int u = v < nv ? v : 0;
			v = u + 1 < nv ? u + 1 : 0;
			int w = v + 1 < nv ? v + 1 : 0;
			if (Snip(u, v, w, nv, V))
			{
				indices.Add(V[u]); indices.Add(V[v]); indices.Add(V[w]);
				for (int s = v, t = v + 1; t < nv; s++, t++) V[s] = V[t];
				nv--; count = 2 * nv;
			}
		}
		return indices.ToArray();
	}

	float Area()
	{
		float A = 0;
		int n = m_pts.Count;
		for (int p = n - 1, q = 0; q < n; p = q++)
		{
			Vector2 P = m_pts[p], Q = m_pts[q];
			A += P.x * Q.y - Q.x * P.y;
		}
		return A * 0.5f;
	}

	bool Snip(int u, int v, int w, int n, int[] V)
	{
		Vector2 A = m_pts[V[u]], B = m_pts[V[v]], C = m_pts[V[w]];
		if (Mathf.Epsilon > ((B.x - A.x) * (C.y - A.y) - (B.y - A.y) * (C.x - A.x)))
			return false;
		for (int p = 0; p < n; p++)
		{
			if (p == u || p == v || p == w) continue;
			if (InsideTriangle(A, B, C, m_pts[V[p]])) return false;
		}
		return true;
	}

	bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
	{
		float ax = C.x - B.x, ay = C.y - B.y;
		float bx = A.x - C.x, by = A.y - C.y;
		float cx = B.x - A.x, cy = B.y - A.y;
		float apx = P.x - A.x, apy = P.y - A.y;
		float bpx = P.x - B.x, bpy = P.y - B.y;
		float cpx = P.x - C.x, cpy = P.y - A.y;
		return (ax * bpy - ay * bpx >= 0) &&
			   (bx * cpy - by * cpx >= 0) &&
			   (cx * apy - cy * apx >= 0);
	}
}