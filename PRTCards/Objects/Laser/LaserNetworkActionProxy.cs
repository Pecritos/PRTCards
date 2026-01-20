using Photon.Pun;
using PRT;
using PRT.Objects.TNT;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class LaserNetworkActionProxy : MonoBehaviourPun
{
    private Dictionary<int, RuntimeMarker> runtimeMarkers = new Dictionary<int, RuntimeMarker>();

    private void Awake()
    {
        if (photonView != null) photonView.RefreshRpcMonoBehaviourCache();

        foreach (var marker in FindObjectsOfType<RuntimeMarker>())
        {
            if (!runtimeMarkers.ContainsKey(marker.RuntimeID))
                runtimeMarkers[marker.RuntimeID] = marker;
        }
    }

    public void RequestCut(GameObject original, Vector2 pointA, Vector2 pointB)
    {
        if (original == null) return;
        if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
        {
            ProcessAndBroadcastCut(original, pointA, pointB);
        }
        else
        {
            PhotonView targetPV = original.GetComponent<PhotonView>();
            var marker = original.GetComponent<RuntimeMarker>();

            if (targetPV != null)
            {
                photonView.RPC("RPC_RequestCutOnMaster", RpcTarget.MasterClient, targetPV.ViewID, pointA, pointB);
            }
            else if (marker != null)
            {
                photonView.RPC("RPC_RequestCutByRuntimeIDOnMaster", RpcTarget.MasterClient, marker.RuntimeID, pointA, pointB);
            }
        }
    }

    [PunRPC]
    private void RPC_RequestCutByRuntimeIDOnMaster(int runtimeID, Vector2 pointA, Vector2 pointB)
    {
        if (runtimeMarkers.TryGetValue(runtimeID, out var marker))
        {
            ProcessAndBroadcastCut(marker.gameObject, pointA, pointB);
        }
    }

    [PunRPC]
    private void RPC_RequestCutOnMaster(int targetViewID, Vector2 pointA, Vector2 pointB)
    {
        PhotonView pv = PhotonView.Find(targetViewID);
        if (pv != null)
        {
            ProcessAndBroadcastCut(pv.gameObject, pointA, pointB);
        }
    }

    [PunRPC]
    private void RPC_SyncRopeCut(int runtimeID)
    {
        if (runtimeMarkers.TryGetValue(runtimeID, out var marker))
        {
            var rope = marker.GetComponent<MapObjet_Rope>() ?? marker.GetComponentInChildren<MapObjet_Rope>();
            if (rope != null)
            {
                var m = typeof(MapObjet_Rope).GetMethod("Leave", BindingFlags.NonPublic | BindingFlags.Instance);
                m?.Invoke(rope, null);
            }
        }
    }

    private void ProcessAndBroadcastCut(GameObject original, Vector2 pointA, Vector2 pointB)
    {
        if (original == null) return;

        var box = original.GetComponent<BoxCollider2D>();
        if (box != null && original.GetComponent<PolygonCollider2D>() == null)
            ConvertBoxToPolygon(box);

        var circle = original.GetComponent<CircleCollider2D>();
        if (circle != null && original.GetComponent<PolygonCollider2D>() == null)
            ConvertCircleToPolygon(circle);

        List<Cutter2DPhoton.CutterPieceData> pieces = Cutter2DPhoton.CutAndReturnData(original, pointA, pointB);

        if (pieces == null || pieces.Count == 0)
        {
            return;
        }

        var marker = original.GetComponent<RuntimeMarker>();
        if (marker != null) EnsurePhotonView(original, marker.RuntimeID);

        PhotonView originalPV = original.GetComponent<PhotonView>();
        int originalViewID = (originalPV != null) ? originalPV.ViewID : -1;
        RuntimeMarker originalmarker = original.GetComponent<RuntimeMarker>();

        for (int i = 0; i < pieces.Count; i++)
        {
            var data = pieces[i];
            int newViewID = PhotonNetwork.AllocateViewID(true);

            float[] flatPoints = new float[data.localPoints.Length * 2];
            for (int j = 0; j < data.localPoints.Length; j++)
            {
                flatPoints[j * 2] = data.localPoints[j].x;
                flatPoints[j * 2 + 1] = data.localPoints[j].y;
            }

            Vector3 rgb = new Vector3(data.color.r, data.color.g, data.color.b);
            int finalLayer = (data.layer == LayerMask.NameToLayer("IgnoreMap")) ? LayerMask.NameToLayer("Default") : data.layer;
            bool withPhys = (finalLayer == LayerMask.NameToLayer("IgnorePlayer")) ||
                           (finalLayer == LayerMask.NameToLayer("BackgroundObject")) ||
                           (finalLayer == LayerMask.NameToLayer("Default"));

            Sprite pieceSprite = Cutter2DPhoton.GetSpriteByID(data.spriteID);
            bool useSpecial = ShouldUseSpecialMaterial(original, pieceSprite);

            photonView.RPC("RPC_SpawnPiece", RpcTarget.All,
                newViewID, flatPoints, data.position, data.rotation, data.scale,
                data.spriteID, rgb, data.color.a,
                finalLayer, data.sortingLayerID, data.sortingOrder,
                withPhys, data.mass, data.gravityScale,
                original.tag, LayerMask.LayerToName(original.layer),
                useSpecial
            );
        }

                if (originalViewID != -1)
        {
            photonView.RPC("RPC_DestroyNetworkPiece", RpcTarget.All, originalViewID);
        }
        else if (originalmarker != null)
        {
            photonView.RPC("RPC_DestroyWithIDPiece", RpcTarget.All, originalmarker.RuntimeID);
        }
        else
        {
                        Destroy(original);
        }
    }

    [PunRPC]
    public void RPC_SpawnPiece(int viewID, float[] flatPoints, Vector3 pos, Quaternion rot, Vector3 scale, int spriteID, Vector3 rgb, float a, int layer, int sLayer, int sOrder, bool withPhys, float mass, float grav, string originalTag, string originLayerName, bool useSpecialMaterial)
    {
        Vector2[] points = new Vector2[flatPoints.Length / 2];
        for (int i = 0; i < points.Length; i++)
            points[i] = new Vector2(flatPoints[i * 2], flatPoints[i * 2 + 1]);

        Sprite foundSprite = Cutter2DPhoton.GetSpriteByID(spriteID);
        GameObject piece = new GameObject("Piece_" + viewID);
        piece.transform.SetPositionAndRotation(pos, rot);
        piece.transform.localScale = scale;
        piece.layer = layer;
        piece.tag = originalTag;

        if (foundSprite != null)
        {
            var pd = piece.AddComponent<PieceData>();
            pd.sprite = foundSprite;
            pd.OriginalLayer = originLayerName;
            pd.createdTime = Time.time;
        }

        var mf = piece.AddComponent<MeshFilter>();
        mf.mesh = Cutter2DPhoton.CreateMeshFromPolygon(points.ToList(), foundSprite);

        var mr = piece.AddComponent<MeshRenderer>();
        if (useSpecialMaterial && Assets.Bundle != null)
        {
            Material specialMat = Assets.Bundle.LoadAsset<Material>("Mat_MeshMaskWrite");
            mr.material = new Material(specialMat);
            mr.material.renderQueue = 4000;
        }
        else
        {
            mr.material = new Material(Shader.Find("Sprites/Default"));
        }

        if (foundSprite != null) mr.material.mainTexture = foundSprite.texture;
        mr.material.color = new Color(rgb.x, rgb.y, rgb.z, a);
        mr.sortingLayerID = sLayer;
        mr.sortingOrder = sOrder;

        var poly = piece.AddComponent<PolygonCollider2D>();
        poly.points = points;

        try
        {
            var sfPoly = piece.AddComponent<SFPolygon>();
            sfPoly.pathCount = 1;
            sfPoly.verts = points;
            sfPoly.looped = true;
            sfPoly._UpdateBounds();
        }
        catch { }

        var pv = piece.AddComponent<PhotonView>();
        pv.ViewID = viewID;
        pv.OwnershipTransfer = OwnershipOption.Request;

        if (withPhys)
        {
            var rb = piece.AddComponent<Rigidbody2D>();
            rb.mass = mass;
            rb.gravityScale = grav;
            var netPhys = piece.AddComponent<NetworkPhysicsObject>();
            netPhys.photonView = pv;
            netPhys.speed = 8000f;
            netPhys.maxShake = 8000000f;
            netPhys.dmgAmount = 0.5f;
            netPhys.bulletPushMultiplier = 20f;
            netPhys.collisionThreshold = 200000f;
            netPhys.forceAmount = 1500f;
            netPhys.playerColThreshold = 5f;
            pv.ObservedComponents = new List<Component> { netPhys };
        }
        piece.AddComponent<DestroyPieceOutOfCamera>();
        SetPhotonInternalFlags(pv);
    }

    [PunRPC]
    public void RPC_DestroyNetworkPiece(int viewID)
    {
        PhotonView target = PhotonView.Find(viewID);
        if (target != null) Destroy(target.gameObject);
    }

    [PunRPC]
    public void RPC_DestroyWithIDPiece(int runtimeID)
    {

        if (runtimeMarkers.TryGetValue(runtimeID, out var marker))
        {
            if (marker != null && marker.gameObject != null)
            {
                                runtimeMarkers.Remove(runtimeID);
                Destroy(marker.gameObject);
            }
        }
    }
    [PunRPC]
    private void RPC_RequestRopeCutOnMaster(int runtimeID)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_SyncRopeCut", RpcTarget.All, runtimeID);
        }
    }



    private PolygonCollider2D ConvertBoxToPolygon(BoxCollider2D box)
    {
        var polygon = box.gameObject.GetComponent<PolygonCollider2D>() ?? box.gameObject.AddComponent<PolygonCollider2D>();
        Vector2 size = box.size;
        Vector2 offset = box.offset;
        Vector2[] points = new Vector2[4];
        points[0] = offset + new Vector2(-size.x, -size.y) * 0.5f;
        points[1] = offset + new Vector2(-size.x, size.y) * 0.5f;
        points[2] = offset + new Vector2(size.x, size.y) * 0.5f;
        points[3] = offset + new Vector2(size.x, -size.y) * 0.5f;
        polygon.pathCount = 1;
        polygon.SetPath(0, points);
        Destroy(box);
        return polygon;
    }

    public void AddMarkerToDictionary(int id, RuntimeMarker marker)
    {
        if (!runtimeMarkers.ContainsKey(id))
        {
            runtimeMarkers[id] = marker;
        }
    }

    private PolygonCollider2D ConvertCircleToPolygon(CircleCollider2D circle, int segments = 16)
    {
        var polygon = circle.gameObject.GetComponent<PolygonCollider2D>() ?? circle.gameObject.AddComponent<PolygonCollider2D>();
        Vector2 center = circle.offset;
        float radius = circle.radius;
        Vector2[] points = new Vector2[segments];
        for (int i = 0; i < segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }
        polygon.pathCount = 1;
        polygon.SetPath(0, points);
        Destroy(circle);
        return polygon;
    }

    private bool ShouldUseSpecialMaterial(GameObject original, Sprite sprite)
    {
        if (sprite == null) return false;
        if (original.layer != LayerMask.NameToLayer("Default")) return false;

        string spriteName = sprite.name.ToLower();
        return spriteName.Contains("square") || spriteName.Contains("circle");
    }

    [PunRPC]
    private void RPC_AssignPhotonView(int runtimeID, int viewID)
    {
        if (runtimeMarkers.TryGetValue(runtimeID, out var marker))
        {
            var pv = marker.gameObject.GetComponent<PhotonView>() ?? marker.gameObject.AddComponent<PhotonView>();
            pv.ViewID = viewID;
        }
    }

    public void EnsurePhotonView(GameObject go, int runtimeID)
    {
        var pv = go.GetComponent<PhotonView>();
        if (pv != null) return;
        pv = go.AddComponent<PhotonView>();
        if (PhotonNetwork.IsMasterClient)
        {
            pv.ViewID = PhotonNetwork.AllocateViewID(true);
            photonView.RPC("RPC_AssignPhotonView", RpcTarget.OthersBuffered, runtimeID, pv.ViewID);
        }
    }

    private void SetPhotonInternalFlags(PhotonView pv)
    {
        var pvType = typeof(PhotonView);
        pvType.GetField("ownerActorNr", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(pv, PhotonNetwork.IsMasterClient ? PhotonNetwork.LocalPlayer.ActorNumber : 0);
        pvType.GetField("<AmOwner>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(pv, PhotonNetwork.IsMasterClient);
        pvType.GetField("amController", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(pv, true);
        pvType.GetField("isRuntimeInstantiated", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(pv, true);
    }
}