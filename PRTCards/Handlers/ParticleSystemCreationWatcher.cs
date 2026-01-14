using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class ParticleSystemCreationWatcher : MonoBehaviour
{
	public static event System.Action<ParticleSystem> OnParticleSystemCreated;

	void Awake()
	{
		ParticleSystem[] all = FindObjectsOfType<ParticleSystem>();
		foreach (var ps in all)
		{
			Notify(ps);
		}
	}

	void OnTransformChildrenChanged()
	{
		ParticleSystem[] all = GetComponentsInChildren<ParticleSystem>();
		foreach (var ps in all)
		{
			Notify(ps);
		}
	}

	void Notify(ParticleSystem ps)
	{
		OnParticleSystemCreated?.Invoke(ps);
	}
}
