using Photon.Pun;
using PRT;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class Cutter2DPhoton
{
    private static List<GameObject> createdPieces = new List<GameObject>();
    private static AssetBundle bundle;
    private static bool usedParentFallback;

    public static List<GameObject> CutAndReturnPieces(GameObject original, Vector2 pointA, Vector2 pointB)
    {
        var col = original.GetComponent<PolygonCollider2D>();
        if (col == null) return null;

        var worldPoints = col.points.Select(p => (Vector2)original.transform.TransformPoint(p)).ToList();
        var side1 = new List<Vector2>();
        var side2 = new List<Vector2>();
        int n = worldPoints.Count;

        for (int i = 0; i < n; i++)
        {
            Vector2 P = worldPoints[i];
            Vector2 Q = worldPoints[(i + 1) % n];
            bool aboveP = IsAbove(P, pointA, pointB);
            bool aboveQ = IsAbove(Q, pointA, pointB);

            if (aboveP) side1.Add(P); else side2.Add(P);

            if (aboveP != aboveQ && SegmentIntersect(P, Q, pointA, pointB, out Vector2 intersection))
            {
                side1.Add(intersection);
                side2.Add(intersection);
            }
        }


        bool originalHasRb = original.GetComponent<Rigidbody2D>() != null;
        int originalLayer = original.layer;
        int defaultLayer = LayerMask.NameToLayer("Default");

        MapObjet_Rope rope = null;
        Rigidbody2D originalRb = original.GetComponent<Rigidbody2D>();
        if (originalRb != null)
        {
            var refComp = originalRb.GetComponent<RigidbodyRopeRef>();
            if (refComp != null)
                rope = refComp.rope;
        }

        List<GameObject> pieces = new List<GameObject>();

        if (side1.Count >= 3)
            pieces.Add(CreateNewPiece(original, side1, originalHasRb || (originalLayer == defaultLayer && !originalHasRb)));

        if (side2.Count >= 3)
        {
            bool givePhysics = originalHasRb || (originalLayer != defaultLayer && originalHasRb);
            if (originalLayer == defaultLayer && !originalHasRb) givePhysics = false;
            pieces.Add(CreateNewPiece(original, side2, givePhysics));
        }

        if (usedParentFallback && original.transform.parent != null)
        {
            GameObject.Destroy(original.transform.parent.gameObject);
        }
        else
        {
            GameObject.Destroy(original);
        }

        usedParentFallback = false;


        if (rope != null)
        {
            var m = typeof(MapObjet_Rope).GetMethod("Leave", BindingFlags.NonPublic | BindingFlags.Instance);
            if (m != null)
            {
                try
                {
                    m.Invoke(rope, null);
                }
                catch (System.Exception ex)
                {
                }
            }
        }

        return pieces;

    }





    private static GameObject CreateNewPiece(GameObject baseObj, List<Vector2> points, bool withPhysics)
    {
        SpriteRenderer srOriginal = baseObj.GetComponent<SpriteRenderer>();
        MeshRenderer mrOriginal = baseObj.GetComponent<MeshRenderer>();
        Sprite spriteForCut = null;

        int originalLayer = baseObj.layer;
        int ignoreMapLayer = LayerMask.NameToLayer("IgnoreMap");

        int pieceLayer = (originalLayer == ignoreMapLayer)
            ? LayerMask.NameToLayer("Default")
            : originalLayer;

        var data = baseObj.GetComponent<PieceData>();
        if (data != null && data.sprite != null)
            spriteForCut = data.sprite;

        if (spriteForCut == null && srOriginal != null)
            spriteForCut = srOriginal.sprite;

        if (spriteForCut == null)
        {
            foreach (Transform child in baseObj.transform)
            {
                var srChild = child.GetComponent<SpriteRenderer>();
                if (srChild != null && srChild.sprite != null)
                {
                    srOriginal = srChild;
                    spriteForCut = srChild.sprite;
                    break;
                }
            }
        }

        if (spriteForCut == null)
        {
            Transform parent = baseObj.transform.parent;
            if (parent != null)
            {
                foreach (Transform sibling in parent)
                {
                    if (sibling == baseObj.transform) continue;

                    var srSibling = sibling.GetComponent<SpriteRenderer>();
                    if (srSibling != null && srSibling.sprite != null)
                    {
                        srOriginal = srSibling;
                        spriteForCut = srSibling.sprite;
                        usedParentFallback = true;
                        break;
                    }

                    var srInChildren = sibling.GetComponentInChildren<SpriteRenderer>();
                    if (srInChildren != null && srInChildren.sprite != null)
                    {
                        srOriginal = srInChildren;
                        spriteForCut = srInChildren.sprite;
                        usedParentFallback = true;
                        break;
                    }
                }
            }
        }

        var colorChild = baseObj.transform.Find("Color");
        if (colorChild != null)
        {
            var srColor = colorChild.GetComponent<SpriteRenderer>();
            if (srColor != null)
                srOriginal = srColor;
        }

        // --- Nova lógica de cor ---
        Color defaultColor = new Color(0.2157f, 0.2157f, 0.2157f, 1f);
        Color actualColor = srOriginal != null ? srOriginal.color : Color.white;
        bool isColoredByColor = !IsColorEqual(actualColor, defaultColor);


        Color? forcedColor = null;
        if (isColoredByColor)
        {
            if (data == null)
            {
                if (srOriginal != null)
                    forcedColor = srOriginal.color;
            }
            else
            {
                var mrBase = baseObj.GetComponent<MeshRenderer>();
                if (mrBase != null && mrBase.material != null)
                    forcedColor = mrBase.material.color;
            }
        }

        string pieceName = (pieceLayer == LayerMask.NameToLayer("IgnorePlayer"))
            ? "Piece"
            : "Piece_IgnoreLava";

        GameObject piece = new GameObject(pieceName);
        piece.transform.position = baseObj.transform.position;
        piece.transform.rotation = baseObj.transform.rotation;
        piece.transform.localScale = baseObj.transform.localScale;
        piece.tag = baseObj.tag;
        piece.layer = pieceLayer;

        if (spriteForCut != null)
        {
            var pd = piece.AddComponent<PieceData>();
            pd.sprite = spriteForCut;
            pd.OriginalLayer = (data != null && !string.IsNullOrEmpty(data.OriginalLayer))
                ? data.OriginalLayer
                : LayerMask.LayerToName(originalLayer);
            pd.createdTime = Time.time;
        }

        for (int i = 0; i < points.Count; i++)
            points[i] = piece.transform.InverseTransformPoint(points[i]);

        Mesh mesh = CreateMeshFromPolygon(points, spriteForCut);
        var mf = piece.AddComponent<MeshFilter>();
        mf.mesh = mesh;
        var mr = piece.AddComponent<MeshRenderer>();

        if (spriteForCut != null)
        {
            PieceData pdBase = baseObj.GetComponent<PieceData>();
            bool useSpecialMaterial;

            if (pdBase == null)
            {
                useSpecialMaterial =
                    originalLayer == LayerMask.NameToLayer("Default") &&
                    (spriteForCut.name.ToLower().Contains("square") ||
                     spriteForCut.name.ToLower().Contains("circe"));
            }
            else
            {
                useSpecialMaterial =
                    pdBase.OriginalLayer == "Default" &&
                    (spriteForCut.name.ToLower().Contains("square") ||
                     spriteForCut.name.ToLower().Contains("circe"));
            }

            // --- só desativa o material especial se a cor for diferente ---
            if (isColoredByColor)
                useSpecialMaterial = false;

            Color? inheritedColor = null;
            if (pdBase != null)
            {
                var mrBase = baseObj.GetComponent<MeshRenderer>();
                if (mrBase != null && mrBase.material != null)
                    inheritedColor = mrBase.material.color;
            }

            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.mainTexture = spriteForCut.texture;

            if (srOriginal != null)
                mat.color = srOriginal.color;

            if (useSpecialMaterial)
            {
                bundle = Assets.Bundle;
                Material maskMaterial = bundle.LoadAsset<Material>("Mat_MeshMaskWrite");
                mr.material = maskMaterial;
                mr.material.renderQueue = 4000;
            }
            else
            {
                mr.material = mat;
            }

            if (forcedColor.HasValue)
                mr.material.color = forcedColor.Value;
            else if (srOriginal != null)
                mr.material.color = srOriginal.color;
            else if (inheritedColor.HasValue)
                mr.material.color = inheritedColor.Value;

            if (srOriginal != null)
            {
                mr.sortingLayerID = srOriginal.sortingLayerID;
                mr.sortingOrder = srOriginal.sortingOrder;
            }
        }
        else if (mrOriginal != null)
        {
            Material mat = new Material(mrOriginal.material.shader);
            mat.mainTexture = mrOriginal.material.mainTexture;
            mat.color = mrOriginal.material.color;

            mr.material = mat;
            mr.sortingLayerID = mrOriginal.sortingLayerID;
            mr.sortingOrder = mrOriginal.sortingOrder;
        }

        var poly = piece.AddComponent<PolygonCollider2D>();
        poly.points = points.ToArray();
        poly.pathCount = 1;

        try
        {
            var sfPoly = piece.AddComponent<SFPolygon>();
            sfPoly.pathCount = 1;
            sfPoly.verts = points.ToArray();
            sfPoly.looped = true;
            sfPoly._UpdateBounds();
        }
        catch (System.Exception) { }

        NetworkPhysicsObject netPhysics = null;

        if (withPhysics)
        {
            var originalRb = baseObj.GetComponent<Rigidbody2D>();
            var newRb = piece.AddComponent<Rigidbody2D>();

            if (originalRb != null)
            {
                newRb.gravityScale = originalRb.gravityScale;
                newRb.mass = originalRb.mass;
                newRb.drag = originalRb.drag;
                newRb.angularDrag = originalRb.angularDrag;
                newRb.constraints = originalRb.constraints;
                newRb.interpolation = originalRb.interpolation;
                newRb.collisionDetectionMode = originalRb.collisionDetectionMode;
                newRb.bodyType = originalRb.bodyType;
            }
            else
            {
                newRb.gravityScale = 1f;
                newRb.mass = 20000f;
            }

            netPhysics = piece.AddComponent<NetworkPhysicsObject>();
        }

        piece.AddComponent<DestroyPieceOutOfCamera>();

        var pv = piece.AddComponent<PhotonView>();
        if (!PhotonNetwork.AllocateViewID(pv))
        {
            GameObject.Destroy(piece);
            return null;
        }

        pv.observableSearch = PhotonView.ObservableSearch.AutoFindAll;
        if (pv.ObservedComponents == null)
            pv.ObservedComponents = new List<Component>();

        if (netPhysics != null)
        {
            if (piece.layer == LayerMask.NameToLayer("Default"))
                piece.layer = LayerMask.NameToLayer("IgnorePlayer");

            netPhysics.speed = 8000f;
            netPhysics.maxShake = 8000000f;
            netPhysics.dmgAmount = 0.5f;
            netPhysics.bulletPushMultiplier = 20f;
            netPhysics.collisionThreshold = 200000f;
            netPhysics.forceAmount = 1500f;
            netPhysics.playerColThreshold = 5f;
            netPhysics.photonView = pv;

            if (!pv.ObservedComponents.Contains(netPhysics))
                pv.ObservedComponents.Add(netPhysics);
        }

        var pvType = typeof(PhotonView);
        pvType.GetField("ownerActorNr", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(pv, PhotonNetwork.LocalPlayer.ActorNumber);
        pvType.GetField("<AmOwner>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(pv, true);
        pvType.GetField("amController", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(pv, true);
        pvType.GetField("isRuntimeInstantiated", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(pv, true);

        createdPieces.Add(piece);
        return piece;
    }



    private static bool IsColorEqual(Color a, Color b, float epsilon = 0.01f)
    {
        return Mathf.Abs(a.r - b.r) < epsilon &&
               Mathf.Abs(a.g - b.g) < epsilon &&
               Mathf.Abs(a.b - b.b) < epsilon &&
               Mathf.Abs(a.a - b.a) < epsilon;
    }



    public static GameObject CreatePieceFromPoints(Vector3 position, Quaternion rotation, Vector3 localScale, int layer, Vector2[] points,
        Sprite sprite, Color color, int sortingLayerID, int sortingOrder, bool withPhysics)
    {
        GameObject piece = new GameObject("Piece");
        piece.transform.position = position;
        piece.transform.rotation = rotation;
        piece.transform.localScale = localScale;
        piece.layer = layer;

        if (sprite != null)
        {
            var pd = piece.AddComponent<PieceData>();
            pd.sprite = sprite;
        }

        Mesh mesh = CreateMeshFromPolygon(points.ToList(), sprite);
        var mf = piece.AddComponent<MeshFilter>();
        mf.mesh = mesh;
        var mr = piece.AddComponent<MeshRenderer>();

        if (sprite != null)
        {
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.mainTexture = sprite.texture;
            mat.color = color;
            mr.material = mat;
            mr.sortingLayerID = sortingLayerID;
            mr.sortingOrder = sortingOrder;
        }
        else
        {
            var mat = new Material(Shader.Find("Sprites/Default"));
            mr.material = mat;
        }

        var poly = piece.AddComponent<PolygonCollider2D>();
        poly.points = points;

        if (withPhysics)
        {
            var newRb = piece.AddComponent<Rigidbody2D>();
            newRb.gravityScale = 1f;
        }

        piece.AddComponent<DestroyPieceOutOfCamera>();

        var pv = piece.AddComponent<PhotonView>();


        createdPieces.Add(piece);

        return piece;
    }

    private static bool IsAbove(Vector2 p, Vector2 a, Vector2 b)
    {
        return ((b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x)) > 0;
    }

    public static bool SegmentIntersect(Vector2 p, Vector2 q, Vector2 a, Vector2 b, out Vector2 r)
    {
        Vector2 v = q - p, w = b - a;
        float d = v.x * w.y - v.y * w.x;
        if (Mathf.Approximately(d, 0f)) { r = default; return false; }
        Vector2 u = a - p;
        float t = (u.x * w.y - u.y * w.x) / d;
        r = p + t * v;
        return t >= 0f && t <= 1f;
    }

    private static Mesh CreateMeshFromPolygon(List<Vector2> pts, Sprite sprite)
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
                Vector3 local = verts[i];
                float px = local.x * ppu + pivot.x;
                float py = local.y * ppu + pivot.y;
                float u = (rect.x + px) / tex.width;
                float v = (rect.y + py) / tex.height;
                uvs[i] = new Vector2(u, v);
            }
        }
        else
        {
            float minX = pts.Min(p => p.x), maxX = pts.Max(p => p.x);
            float minY = pts.Min(p => p.y), maxY = pts.Max(p => p.y);
            float w = maxX - minX, h = maxY - minY;

            for (int i = 0; i < pts.Count; i++)
                uvs[i] = new Vector2(
                    (pts[i].x - minX) / w,
                    (pts[i].y - minY) / h
                );
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