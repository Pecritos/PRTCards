using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GunAdditionalData
{
	public bool canShoot = true;
}

public static class GunExtension
{
	private static readonly ConditionalWeakTable<Gun, GunAdditionalData> data =
		new ConditionalWeakTable<Gun, GunAdditionalData>();

	public static GunAdditionalData GetAdditionalData(this Gun gun)
	{
		return data.GetOrCreateValue(gun);
	}
}
