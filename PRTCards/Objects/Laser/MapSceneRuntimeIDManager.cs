using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;

public class MapSceneRuntimeIDManager : MonoBehaviour
{
	private static int RuntimeCounter = 0;


	public static int RegisterDynamicObject(GameObject go)
	{
		if (go.GetComponent<RuntimeMarker>() == null)
		{
			var marker = go.AddComponent<RuntimeMarker>();
			marker.RuntimeID = ++RuntimeCounter;
			return marker.RuntimeID;
		}
		return go.GetComponent<RuntimeMarker>().RuntimeID;
	}

	void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		bool isMapScene = scene.GetRootGameObjects()
					   .Any(go => go.GetComponentInChildren<Map>() != null);
		if (!isMapScene) return;

		RuntimeCounter = 0;

		StartCoroutine(RegisterAllObjectsWithDelay(scene, 0.5f));
	}

	private IEnumerator RegisterAllObjectsWithDelay(Scene scene, float delay)
	{
		yield return new WaitForSeconds(delay);

		foreach (var go in scene.GetRootGameObjects())
			RegisterMapObjectRecursively(go);
	}

	private void RegisterMapObjectRecursively(GameObject go)
	{
		if (go.GetComponent<RuntimeMarker>() == null)
		{
			var marker = go.AddComponent<RuntimeMarker>();
			marker.RuntimeID = ++RuntimeCounter;
		}

		foreach (Transform child in go.transform)
			RegisterMapObjectRecursively(child.gameObject);
	}
}

public class RuntimeMarker : MonoBehaviour
{
	public int RuntimeID;
}
