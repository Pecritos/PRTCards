using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(PlayerCollision), "FixedUpdate")]
public class Patch_PlayerCollision_NoTrainOwnerCollision
{
	static bool Prefix(PlayerCollision __instance)
	{
		var data = AccessTools
			.Field(typeof(PlayerCollision), "data")
			.GetValue(__instance) as CharacterData;

		if (data == null) return true;

		var lastPos = (Vector2)AccessTools
			.Field(typeof(PlayerCollision), "lastPos")
			.GetValue(__instance);

		var cirCol = __instance.GetComponent<CircleCollider2D>();
		if (cirCol == null) return true;

		float radius = cirCol.radius * __instance.transform.localScale.x;

		var mask = (LayerMask)AccessTools
			.Field(typeof(PlayerCollision), "mask")
			.GetValue(__instance);

		Vector2 dir = (Vector2)__instance.transform.position - lastPos;
		float dist = dir.magnitude;

		if (dist <= 0.0001f) return true;

		var hits = Physics2D.CircleCastAll(lastPos, radius, dir, dist, mask);

		foreach (var hit in hits)
		{
			if (!hit.transform) continue;

			var owner = hit.transform.GetComponentInParent<TrainOwner>();
			if (owner != null && owner.owner == data.player)
			{
				return false;
			}
		}

		return true;
	}
}
