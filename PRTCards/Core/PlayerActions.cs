using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using InControl;
using UnityEngine;
using UnboundLib;

namespace PRT.Core
{
	[Serializable]
	public class PlayerActionsAdditionalData
	{
		public PlayerAction switchWeapon;
	}

	public static class PlayerActionsExtension
	{
		private static readonly ConditionalWeakTable<PlayerActions, PlayerActionsAdditionalData> data =
			new ConditionalWeakTable<PlayerActions, PlayerActionsAdditionalData>();

		public static PlayerActionsAdditionalData GetAdditionalData(this PlayerActions playerActions)
		{
			return data.GetOrCreateValue(playerActions);
		}
	}

	[HarmonyPatch(typeof(PlayerActions))]
	[HarmonyPatch(MethodType.Constructor)]
	class PlayerActionsPatchConstructor
	{
		private static void Postfix(PlayerActions __instance)
		{
			__instance.GetAdditionalData().switchWeapon =
				(PlayerAction)typeof(PlayerActions).InvokeMember("CreatePlayerAction",
				BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic,
				null, __instance, new object[] { "Switch Laser Mode" });
		}
	}

	[HarmonyPatch(typeof(PlayerActions), "CreateWithControllerBindings")]
	class PlayerActionsPatchControllerBindings
	{
		private static void Postfix(ref PlayerActions __result)
		{
			__result.GetAdditionalData().switchWeapon.AddDefaultBinding(InputControlType.DPadLeft);
		}
	}

	[HarmonyPatch(typeof(PlayerActions), "CreateWithKeyboardBindings")]
	class PlayerActionsPatchKeyboardBindings
	{
		private static void Postfix(ref PlayerActions __result)
		{
			__result.GetAdditionalData().switchWeapon.AddDefaultBinding(Key.C);
		}
	}
}
