using HarmonyLib;
using UnityEngine;

public class RigidbodyRopeRef : MonoBehaviour
{
    public MapObjet_Rope rope;
}

[HarmonyPatch(typeof(MapObjet_Rope), "AddJoint")]
public class MapRope_AddJoint_Patch
{
    static void Postfix(MapObjet_Rope __instance, Rigidbody2D target)
    {
        if (target != null)
        {
            RigidbodyRopeRef refComp = target.GetComponent<RigidbodyRopeRef>();
            if (refComp == null)
                refComp = target.gameObject.AddComponent<RigidbodyRopeRef>();

            refComp.rope = __instance;
        }
    }
}
