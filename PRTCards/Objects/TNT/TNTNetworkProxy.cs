using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using PRT.Objects.TNT;
using System.Linq; 
public class TNTNetworkProxy : MonoBehaviourPun
{
    public static TNTNetworkProxy Instance;

    void Awake()
    {
        Instance = this;
    }

    public void RequestNetworkSync(int principalViewID, int[] cloneIDs, Vector2[] clonePositions, int principalInstanceID, float maxLoops, float scale, int spawnerID)
    {
                photonView.RPC("RPC_FinalNetworkSync", RpcTarget.All, principalViewID, cloneIDs, clonePositions, principalInstanceID, maxLoops, scale, spawnerID);
    }

    [PunRPC]
    void RPC_FinalNetworkSync(int principalViewID, int[] cloneIDs, Vector2[] clonePositions, int principalInstanceID, float maxLoops, float scale, int spawnerID)
    {
        var spawnerPlayer = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == spawnerID);
        GameObject prefab = TNTSpawner.GetPrefab();

                TNTScript principal = FindObjectsOfType<TNTScript>()
                                .FirstOrDefault(t => !t.isCopy && (t.GetComponent<PhotonView>() == null || t.GetComponent<PhotonView>().ViewID <= 0));

        if (principal != null)
        {
            SetupNetworking(principal.gameObject, principalViewID);
        }

                for (int i = 0; i < cloneIDs.Length; i++)
        {
            GameObject copy = Instantiate(prefab, clonePositions[i], Quaternion.identity);

                        var sr = copy.transform.Find("SpriteRenderer")?.GetComponent<SpriteRenderer>();
            TNTScript script = copy.GetComponent<TNTScript>() ?? copy.AddComponent<TNTScript>();

                        script.isCopy = true;
            script.player_spawner = spawnerPlayer;
            script.tntScale = scale;

                        copy.layer = LayerMask.NameToLayer("PlayerObjectCollider");
            if (sr != null) sr.gameObject.layer = LayerMask.NameToLayer("Default");

                        script.StartTNT(0, maxLoops, true);

                        SetupNetworking(copy, cloneIDs[i]);
        }
    }

    private void SetupNetworking(GameObject obj, int viewID)
    {
                PhotonView pv = obj.GetComponent<PhotonView>() ?? obj.AddComponent<PhotonView>();

                pv.ViewID = viewID;

                NetworkPhysicsObject npo = obj.GetComponent<NetworkPhysicsObject>() ?? obj.AddComponent<NetworkPhysicsObject>();

                pv.ObservedComponents = new List<Component> { npo };
        pv.Synchronization = ViewSynchronization.UnreliableOnChange;

                Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;         }
    }
}