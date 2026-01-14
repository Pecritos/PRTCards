using HarmonyLib;
using UnityEngine;
using System;
using System.Collections;
using System.Reflection;

[HarmonyPatch(typeof(WWMO.MonoBehaviours.WaterMono), "HandleBox")]
[HarmonyPatch(new Type[] { typeof(Rigidbody2D) })]
public class PatchWaterMono_HandleBox
{
	static bool Prefix(Rigidbody2D rb)
	{
		if (rb.gameObject.name.Contains("IgnoreWater"))
		{
			return false;
		}
		return true;
	}
}

[HarmonyPatch(typeof(WWMO.MonoBehaviours.BoxTouchingLava_Mono), "FixedUpdate")]
public class Patch_BoxTouchingLava
{
	static bool Prefix(WWMO.MonoBehaviours.BoxTouchingLava_Mono __instance)
	{
		if (!__instance.gameObject.name.Contains("IgnoreWater") &&
			!__instance.gameObject.name.Contains("IgnoreLava"))
			return true;

		if (__instance.gameObject.name.Contains("IgnoreLava"))
			return false;

		var selfCollider = __instance.GetComponent<Collider2D>();
		if (selfCollider == null) return false;

		var filter = new ContactFilter2D { useTriggers = false };
		var results = new Collider2D[10];
		int count = selfCollider.OverlapCollider(filter, results);

		for (int i = 0; i < count; i++)
		{
			var player = results[i]?.GetComponent<Player>();
			if (player != null)
			{
				player.data.healthHandler.TakeDamageOverTime(
					Vector2.up * 0.25f * __instance.heatPercent,
					Vector2.zero,
					5f, 0.1f,
					new Color(1f, 0f, 0f, 0.7f),
					null, null, true
				);
			}
		}

		return false;
	}
}

[HarmonyPatch(typeof(WWMO.MonoBehaviours.LavaMono), "HandlePlayer")]
[HarmonyPatch(new Type[] { typeof(Player) })]
public class Patch_LavaMono_HandlePlayer
{
	static bool Prefix(WWMO.MonoBehaviours.LavaMono __instance, Player player)
	{
		var hh = player.data.healthHandler;

		hh.TakeDamage(
	Vector2.up * 0.65f,
	player.transform.position,
	null,
	null,
	true
);

		var field = typeof(HealthHandler).GetField("activeDoTs",
	BindingFlags.NonPublic | BindingFlags.Instance);

		var list = (IList)field.GetValue(hh);

		while (list.Count > 1)
		{
			list.RemoveAt(0);
		}

		if (list.Count >= 1)
		{
			return false;
		}

		hh.TakeDamageOverTime(
	Vector2.up * 0.65f,
	Vector2.zero,
	2.5f,
	1f,
	new Color(1f, 0f, 0f, 0.7f),
	null, null, true
);

		return false;
	}
}

