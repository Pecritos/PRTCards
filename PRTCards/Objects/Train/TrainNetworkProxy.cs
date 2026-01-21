using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using PRT.Objects.Train;
using System.Linq;

public class TrainNetworkProxy : MonoBehaviourPun
{
    public static TrainNetworkProxy Instance;
    void Awake() => Instance = this;

    public void RequestTrainSync(int viewID, Vector3 pos, Quaternion rot, Vector2 velocity, float scale, int spawnerID, int direction, bool boomerang, int wagons, bool lava)
    {
        photonView.RPC("RPC_SpawnTrain", RpcTarget.All, viewID, pos, rot, velocity, scale, spawnerID, direction, boomerang, wagons, lava);
    }

    [PunRPC]
    void RPC_SpawnTrain(int viewID, Vector3 pos, Quaternion rot, Vector2 velocity, float scale, int spawnerID, int direction, bool boomerang, int wagons, bool lava)
    {
        var player = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == spawnerID);
        TrainSpawner.InternalNetworkSpawn(viewID, pos, rot, velocity, scale, player, (TrainSpawner.TrainDirection)direction, boomerang, wagons, lava);
    }
}