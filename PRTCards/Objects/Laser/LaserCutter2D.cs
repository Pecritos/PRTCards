
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;

public class LaserCutter2D : MonoBehaviourPun
{
    [Header("References")]
    public LineRenderer line;
    public LineRenderer visibleline;

    [Header("Hit Info (debug / efeitos)")]
    public HitInfo currentHitInfo;


    [Header("Cut Settings")]
    public string[] cutLayerNames = new string[] { "Default", "IgnorePlayer", "IgnoreMap", "BackgroundObject", "Player" };

    [HideInInspector]
    public Transform Gun2;

    private LayerMask cutLayers;
    private Vector3 startPoint;
    private Vector3 endPoint;
    public bool cutting;
    private float laserLength = 10000000f;

    private Gun gun;

    private bool locked = false;
    private Vector3 lockeddirection;
    private HashSet<Player> Playersdmg = new HashSet<Player>();


    void Start()
    {
        cutLayers = LayerMask.GetMask(cutLayerNames);
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.enabled = false;
        gameObject.tag = "Laser";

        if (visibleline == null)
        {
            GameObject obj = new GameObject("LineSempreVisivel");
            obj.transform.SetParent(transform);
            visibleline = obj.AddComponent<LineRenderer>();

            visibleline.startWidth = 0.05f;
            visibleline.endWidth = 0.05f;
            visibleline.material = line.material;
            visibleline.startColor = new Color(1f, 1f, 1f, 0.3f);
            visibleline.endColor = new Color(1f, 1f, 1f, 0.3f);
            visibleline.positionCount = 2;
            visibleline.enabled = false;
        }

        gun = GetComponentInParent<Gun>();
        if (Gun2 == null && gun != null)
            Gun2 = gun.shootPosition;

        Camera cam = Camera.main;
        if (cam != null && cam.orthographic)
        {
            float altura = cam.orthographicSize * 2f;
            float largura = altura * cam.aspect;
            float diagonal = Mathf.Sqrt(largura * largura + altura * altura);
            laserLength = diagonal + 100f;
        }
        else if (cam != null)
        {
            float height = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * 10f;
            float width = height * cam.aspect;
            float diagonal = Mathf.Sqrt(width * width + height * height);
            laserLength = diagonal + 100f;
        }

        cutting = false;
        enabled = true;
    }

    void Update()
    {
        if (Gun2 == null || gun == null) return;

        if (!locked)
        {
            startPoint = Gun2.position;
            lockeddirection = GetDirecaoDisparo();
            endPoint = startPoint + lockeddirection * laserLength;
        }

        if (!cutting && visibleline.enabled)
        {
            visibleline.SetPosition(0, startPoint);
            visibleline.SetPosition(1, endPoint);
            line.enabled = false;
        }
        else if (cutting)
        {
            visibleline.enabled = false;
            line.enabled = true;
            line.SetPosition(0, startPoint);
            line.SetPosition(1, endPoint);

            UpdateLaserHitEffect(startPoint, lockeddirection);

            CutObjects();
        }
        else
        {
            visibleline.enabled = false;
            line.enabled = false;
        }
    }

    void UpdateLaserHitEffect(Vector3 start, Vector3 dir)
    {
        Camera cam = Camera.main;
        if (cam == null)
            return;

        Vector3 finalPoint = GetLaserCameraLimit(cam, start, dir);

        HitInfo info = new HitInfo
        {
            point = finalPoint,
            normal = -dir,
            collider = null,
            transform = null
        };

        currentHitInfo = info;

        DynamicParticles.instance.PlayBulletHit(
            10000f,
            transform,
            info,
            Color.red
        );
    }



    Vector3 GetLaserCameraLimit(Camera cam, Vector3 start, Vector3 dir)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);

        float minDist = float.MaxValue;
        Vector3 bestPoint = start;

        foreach (var plane in planes)
        {
            if (!plane.Raycast(new Ray(start, dir), out float enter))
                continue;

            if (enter > 0f && enter < minDist)
            {
                minDist = enter;
                bestPoint = start + dir * enter;
            }
        }

        return bestPoint - dir;
    }




    public void ActivateLaser(Transform arma)
    {
        this.Gun2 = arma;

        Playersdmg.Clear();

        cutting = true;
        enabled = true;

        locked = true;
        startPoint = arma.position;
        lockeddirection = GetDirecaoDisparo();
        var direction = GetDirecaoDisparo();
        GamefeelManager.GameFeel(direction * 30);
        endPoint = startPoint + lockeddirection * laserLength;
    }

    public void SwitchOffLaser()
    {
        cutting = false;
        enabled = true;
        SetAimLaser(true);

        locked = false;
    }

    public void SetAimLaser(bool ativa)
    {
        if (visibleline != null)
            visibleline.enabled = ativa;
    }

    private Vector3 GetDirecaoDisparo()
    {
        Quaternion shootRotation = (Quaternion)typeof(Gun).InvokeMember(
            "getShootRotation",
            BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic,
            null,
            gun,
            new object[] { 0, 0, 0f }
        );

        return shootRotation * Vector3.forward;
    }


    void DamagePlayerPercent(Player targetPlayer, float percent)
    {
        if (targetPlayer == null) return;

        var characterData = targetPlayer.GetComponent<CharacterData>();
        var damagable = targetPlayer.GetComponent<Damagable>();

        if (characterData == null || damagable == null)
            return;

        percent = Mathf.Clamp(percent, 0.1f, 1f);

        float damageAmount = characterData.health * percent;
        if (damageAmount <= 0f)
            return;

        Vector2 dir = (targetPlayer.transform.position - startPoint).normalized;

        damagable.CallTakeDamage(
            dir * damageAmount,
            targetPlayer.transform.position,
            null,
            gun.player,
            true
        );
    }


    bool HitConsomePotencia(Collider2D col)
    {
        int layer = col.gameObject.layer;

        return layer == LayerMask.NameToLayer("Player")
            || layer == LayerMask.NameToLayer("Default")
            || layer == LayerMask.NameToLayer("IgnorePlayer");
    }


    void CutObjects()
    {
        
        

        Vector2 dir = (endPoint - startPoint).normalized;
        float dist = laserLength;
        foreach (var h in Physics2D.RaycastAll(startPoint, (endPoint - startPoint).normalized, laserLength, cutLayers))
            




        foreach (var rope in RopeColliderGenerator.AllRopes)
        {
            if (rope == null) continue;

            if (rope.CheckLaserHit(startPoint, dir, dist, out var hitPos))
            {
                var map = rope.GetComponent<MapObjet_Rope>();
                if (map != null)
                {
                    var m = typeof(MapObjet_Rope).GetMethod("Leave",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                    if (m != null)
                        m.Invoke(map, null);
                }
            }
        }

        RaycastHit2D[] hits = Physics2D.RaycastAll(startPoint, dir, dist, cutLayers);

        if (PhotonNetwork.IsMasterClient)
        {
            float currentPercent = 0.9f;

            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;
                if (hit.collider.isTrigger) continue;

                bool consomePotencia = HitConsomePotencia(hit.collider);

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    var ownerPlayer = gun?.player;
                    var effects = ownerPlayer != null ? ownerPlayer.GetComponent<BlockSpawnerEffects>() : null;

                    if (effects == null || !effects.LaserDoDmg)
                        continue;
                    var player = hit.collider.GetComponentInParent<Player>();
                    if (player != null && !Playersdmg.Contains(player))
                    {
                        Playersdmg.Add(player);

                        float percent = Mathf.Max(currentPercent, 0.1f);
                        DamagePlayerPercent(player, percent);
                    }
                }


                if (consomePotencia)
                {
                    currentPercent -= 0.1f;
                    currentPercent = Mathf.Max(currentPercent, 0.1f);
                }
            }
        }




        List<GameObject> targets = new List<GameObject>();

        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;

            if (hit.collider.isTrigger) continue;

            var target = hit.collider.gameObject;

            if (target.layer == LayerMask.NameToLayer("Player"))
                continue;

            if (!targets.Contains(target))
                targets.Add(target);

        }

        foreach (var target in targets.ToList())
        {
            try
            {
                var pd = target.GetComponent<PieceData>();
                if (pd != null && Time.time - pd.createdTime < 0.5f)
                {
                    continue;
                }

                var poly = target.GetComponent<PolygonCollider2D>();
                if (poly == null)
                {
                    var box = target.GetComponent<BoxCollider2D>();
                    if (box != null)
                        poly = ConvertBoxToPolygon(box);
                    else
                    {
                        var circle = target.GetComponent<CircleCollider2D>();
                        if (circle != null)
                            poly = ConvertCircleToPolygon(circle);
                    }
                }

                var meshFilter = target.GetComponent<MeshFilter>();
                if (poly == null && meshFilter == null)
                    continue;

                if (!PhotonNetwork.IsMasterClient)
                    continue;

                var pieces = Cutter2DPhoton.CutAndReturnPieces(target, startPoint, endPoint);
                if (pieces == null || pieces.Count == 0)
                    continue;

                foreach (var col in target.GetComponents<Collider2D>())
                    if (col != null) col.enabled = false;

                foreach (var piece in pieces)
                {
                    try
                    {
                        var pv = piece.GetComponent<PhotonView>();
                        if (pv == null)
                        {
                            continue;
                        }

                        pv.TransferOwnership(photonView.OwnerActorNr);

                        var sprite = piece.GetComponent<PieceData>()?.sprite;
                        if (sprite == null)
                        {
                            continue;
                        }

                        Texture2D tex = sprite.texture;
                        if (tex == null)
                        {
                            continue;
                        }

                        Rect rect = sprite.rect;
                        Vector2 pivot = sprite.pivot;
                        float ppu = sprite.pixelsPerUnit;

                        Color color = piece.GetComponent<MeshRenderer>().material.color;
                        int sortingLayerID = piece.GetComponent<MeshRenderer>().sortingLayerID;
                        int sortingOrder = piece.GetComponent<MeshRenderer>().sortingOrder;

                        Vector3 pos = piece.transform.position;
                        Quaternion rot = piece.transform.rotation;
                        Vector3 scale = piece.transform.localScale;
                        int layer = piece.layer;

                        Vector2[] points = piece.GetComponent<PolygonCollider2D>().points;

                        photonView.RPC("RPC_CreatePieceWithSpriteData", RpcTarget.OthersBuffered,
                            pos, rot, scale, layer,
                            points,
                            tex.EncodeToPNG(),
                            rect.x, rect.y, rect.width, rect.height,
                            pivot.x, pivot.y,
                            ppu,
                            color, sortingLayerID, sortingOrder,
                            true,
                            photonView.OwnerActorNr,
                            pv.ViewID
                        );
                    }
                    catch (System.Exception e)
                    {
                    }
                }
            }
            catch (System.Exception e)
            {
            }
        }
    }


    PolygonCollider2D ConvertCircleToPolygon(CircleCollider2D circle, int segments = 16)
    {
        if (circle == null) return null;

        var polygon = circle.gameObject.GetComponent<PolygonCollider2D>();
        if (polygon == null)
            polygon = circle.gameObject.AddComponent<PolygonCollider2D>();

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

        return polygon;
    }

    PolygonCollider2D ConvertBoxToPolygon(BoxCollider2D box)
    {
        if (box == null) return null;

        var polygon = box.gameObject.GetComponent<PolygonCollider2D>();
        if (polygon == null)
            polygon = box.gameObject.AddComponent<PolygonCollider2D>();

        Vector2 size = box.size;
        Vector2 offset = box.offset;

        Vector2[] points = new Vector2[4];
        points[0] = offset + new Vector2(-size.x, -size.y) * 0.5f;
        points[1] = offset + new Vector2(-size.x, size.y) * 0.5f;
        points[2] = offset + new Vector2(size.x, size.y) * 0.5f;
        points[3] = offset + new Vector2(size.x, -size.y) * 0.5f;

        polygon.pathCount = 1;
        polygon.SetPath(0, points);

        return polygon;
    }

    [PunRPC]
    void RPC_CreatePieceWithSpriteData(Vector3 position, Quaternion rotation, Vector3 localScale, int layer,
Vector2[] points,
byte[] texBytes,
float rectX, float rectY, float rectW, float rectH,
float pivotX, float pivotY,
float ppu,
Color color, int sortingLayerID, int sortingOrder,
bool withPhysics,
int ownerActorNr,
int viewID)
    {
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(texBytes);
        Rect rect = new Rect(rectX, rectY, rectW, rectH);
        Vector2 pivot = new Vector2(pivotX, pivotY);

        Sprite sprite = Sprite.Create(tex, rect, pivot, ppu);

        var piece = Cutter2DPhoton.CreatePieceFromPoints(position, rotation, localScale, layer,
            points, sprite, color, sortingLayerID, sortingOrder, withPhysics);

        if (piece != null)
        {
            var pv = piece.GetComponent<PhotonView>();
            if (pv != null)
            {
                pv.ViewID = viewID;
            }
        }
    }
}