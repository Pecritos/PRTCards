using PRT;
using UnityEngine;

public class ParticleAutoRenderer : MonoBehaviour
{
	private static AssetBundle bundle;

	void Awake()
	{
		bundle = Assets.Bundle; Material maskMaterial = bundle.LoadAsset<Material>("Mat_MeshMaskWrite");

		ApplyToAllExisting(maskMaterial);
	}

	void OnEnable()
	{
		ParticleSystemCreationWatcher.OnParticleSystemCreated += OnParticleSystemCreated;
	}

	void OnDisable()
	{
		ParticleSystemCreationWatcher.OnParticleSystemCreated -= OnParticleSystemCreated;
	}

	void ApplyToAllExisting(Material maskMaterial)
	{
		ParticleSystem[] systems = FindObjectsOfType<ParticleSystem>();
		foreach (var ps in systems)
		{
			DuplicateRenderer(ps, maskMaterial);
		}
	}

	void OnParticleSystemCreated(ParticleSystem ps)
	{
		Material maskMaterial = bundle.LoadAsset<Material>("Mat_MeshMaskWrite");
		DuplicateRenderer(ps, maskMaterial);
	}

	void DuplicateRenderer(ParticleSystem original, Material maskMaterial)
	{
		if (original == null) return;

		GameObject copy = new GameObject(original.name + "_Mask");
		copy.transform.SetParent(original.transform, false);
		copy.transform.localPosition = Vector3.zero;
		copy.transform.localRotation = Quaternion.identity;
		copy.transform.localScale = Vector3.one;

		var newRenderer = copy.AddComponent<ParticleSystemRenderer>();
		var oldRenderer = original.GetComponent<ParticleSystemRenderer>();

		if (oldRenderer != null)
		{
			newRenderer.renderMode = oldRenderer.renderMode;
			newRenderer.mesh = oldRenderer.mesh;
			newRenderer.sortingLayerID = oldRenderer.sortingLayerID;
			newRenderer.sortingOrder = oldRenderer.sortingOrder + 1; newRenderer.material = maskMaterial;
			newRenderer.trailMaterial = oldRenderer.trailMaterial;
			newRenderer.normalDirection = oldRenderer.normalDirection;
			newRenderer.cameraVelocityScale = oldRenderer.cameraVelocityScale;
			newRenderer.velocityScale = oldRenderer.velocityScale;
			newRenderer.lengthScale = oldRenderer.lengthScale;

			newRenderer.alignment = oldRenderer.alignment;
		}
	}
}
