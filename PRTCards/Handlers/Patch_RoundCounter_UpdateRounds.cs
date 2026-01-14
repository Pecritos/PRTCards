using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(Map), "MapMoveOut")]
class Patch_Map_MapMoveOut
{
    static void Prefix()
    {
        foreach (var go in GameObject.FindObjectsOfType<GameObject>())
        {
            if (go.name.Contains("Piece"))
                Object.Destroy(go);
        }
    }
}
