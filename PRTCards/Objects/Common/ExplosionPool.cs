using System.Collections.Generic;
using System.Reflection;
using PRT;
using PRT.Objects.TNT;
using UnityEngine;

public class ExplosionPool : MonoBehaviour
{
	public static ExplosionPool Instance;

	public GameObject effectPrefab;
	public List<GameObject> pool = new List<GameObject>();
	public int poolSize = 40;

	private GameObject container;
	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);

			AssetBundle bundle = Assets.Bundle;
			if (effectPrefab == null && bundle != null)
				effectPrefab = bundle.LoadAsset<GameObject>("explosion");

			if (effectPrefab == null)
			{
				var fallbackBundle = typeof(TNTSpawner).GetField("bundle", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as AssetBundle;
				if (fallbackBundle != null)
					effectPrefab = fallbackBundle.LoadAsset<GameObject>("explosion");
			}


			container = new GameObject("ExplosionsContainer");
			container.transform.SetParent(transform);

			if (effectPrefab != null)
			{
				for (int i = 0; i < poolSize; i++)
				{
					var obj = Instantiate(effectPrefab);
					obj.SetActive(false);

					obj.transform.SetParent(container.transform);

					pool.Add(obj);
				}
			}
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public GameObject GetExplosion()
	{
		foreach (var obj in pool)
		{
			if (!obj.activeInHierarchy)
				return obj;
		}

		GameObject newObj = Instantiate(effectPrefab);
		newObj.SetActive(false);

		newObj.transform.SetParent(container.transform);

		pool.Add(newObj);
		return newObj;
	}

	public void RegisterExplosion(GameObject explosion)
	{
		explosion.transform.SetParent(container.transform);
		pool.Add(explosion);
	}

	public void ReturnToContainer(GameObject explosion)
	{
		if (container != null)
			explosion.transform.SetParent(container.transform);
	}


}
