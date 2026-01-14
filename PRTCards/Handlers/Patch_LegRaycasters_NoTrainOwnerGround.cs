using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(LegRaycasters), "HitGround")]
public class Patch_LegRaycasters_NoTrainOwnerGround
{
	static bool Prefix(LegRaycasters __instance, RaycastHit2D hit)
	{
		var data = AccessTools
			.Field(typeof(LegRaycasters), "data")
			.GetValue(__instance) as CharacterData;

		if (data == null) return true;
		if (!hit.transform) return true;

		var owner = hit.transform.GetComponentInParent<TrainOwner>();
		if (owner != null && owner.owner == data.player)
		{
			return false;
		}

		return true;
	}
}
