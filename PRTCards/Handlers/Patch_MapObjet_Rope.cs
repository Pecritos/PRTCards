using HarmonyLib;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

[HarmonyPatch(typeof(MapObjet_Rope))]
[HarmonyPatch("Update")]
class MapObjetRope_Update_Patch
{
	static Dictionary<MapObjet_Rope, bool> originallyTwoPointsMap = new Dictionary<MapObjet_Rope, bool>();

	static bool Prefix(MapObjet_Rope __instance)
	{
		if (__instance == null) return true;

		var type = typeof(MapObjet_Rope);
		var jointField = type.GetField("joint", BindingFlags.NonPublic | BindingFlags.Instance);
		var lrField = type.GetField("lineRenderer", BindingFlags.NonPublic | BindingFlags.Instance);

		var joint = jointField?.GetValue(__instance) as AnchoredJoint2D;
		var lr = lrField?.GetValue(__instance) as LineRenderer;

		if (joint == null || lr == null) return true;

		if (!originallyTwoPointsMap.ContainsKey(__instance))
		{
			bool hasTwoPoints = joint.attachedRigidbody != null && joint.connectedBody != null;
			originallyTwoPointsMap[__instance] = hasTwoPoints;
		}

		bool originallyTwoPoints = originallyTwoPointsMap[__instance];

		if (originallyTwoPoints)
		{
			bool attachedMissing = joint.attachedRigidbody == null || !joint.attachedRigidbody.gameObject.activeInHierarchy;
			bool connectedMissing = joint.connectedBody == null || !joint.connectedBody.gameObject.activeInHierarchy;

			if (attachedMissing || connectedMissing)
			{
				if (lr.enabled)
				{
					lr.enabled = false;
				}
			}
		}

		return true;
	}
}
