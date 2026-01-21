using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Photon.Pun;

public class LaserCutter2D : MonoBehaviour
{
	[Header("References")]
	public LineRenderer line;
	public LineRenderer visibleline;

	[Header("Hit Info")]
	public HitInfo currentHitInfo;

	[Header("Cut Settings")]
	public string[] cutLayerNames = new string[] { "Default", "IgnorePlayer", "IgnoreMap", "BackgroundObject", "Player" };

	[HideInInspector]
	public Transform Gun2;
	public LaserNetworkActionProxy cutterProxy;

	private LayerMask cutLayers;
	private Vector3 startPoint;
	private Vector3 endPoint;
	public bool cutting;
	private float laserLength = 100f;

	private Gun gun;
	private bool locked = false;
	private Vector3 lockeddirection;
	private HashSet<GameObject> alreadyRequestedThisCut = new HashSet<GameObject>();

	void Start()
	{
		cutLayers = LayerMask.GetMask(cutLayerNames);
		line = GetComponent<LineRenderer>();
		line.positionCount = 2;
		line.enabled = false;
		gameObject.tag = "Laser";

		if (visibleline == null)
		{
			GameObject obj = new GameObject("LineSempreVisivel");
			obj.transform.SetParent(transform);
			visibleline = obj.AddComponent<LineRenderer>();
			visibleline.startWidth = 0.05f;
			visibleline.endWidth = 0.05f;
			visibleline.material = line.material;
			visibleline.startColor = new Color(1f, 1f, 1f, 0.3f);
			visibleline.endColor = new Color(1f, 1f, 1f, 0.3f);
			visibleline.positionCount = 2;
			visibleline.enabled = false;
		}

		gun = GetComponentInParent<Gun>();
		if (Gun2 == null && gun != null)
			Gun2 = gun.shootPosition;

		if (cutterProxy == null && gun != null && gun.player != null)
		{
			cutterProxy = gun.player.GetComponent<LaserNetworkActionProxy>();
		}

		CalculateLaserLength();
		cutting = false;
	}

	void CalculateLaserLength()
	{
		Camera cam = Camera.main;
		if (cam == null) return;

		if (cam.orthographic)
		{
			float altura = cam.orthographicSize * 2f;
			laserLength = (altura * cam.aspect) + 100f;
		}
		else
		{
			laserLength = 500f;
		}
	}

	void Update()
	{
		if (Gun2 == null || gun == null) return;

		if (!locked)
		{
			startPoint = Gun2.position;
			lockeddirection = GetFireDirection();
			endPoint = startPoint + lockeddirection * laserLength;
		}

		if (cutting)
		{
			visibleline.enabled = false;
			line.enabled = true;
			line.SetPosition(0, startPoint);
			line.SetPosition(1, endPoint);

			if (gun.player != null && gun.player.data.view.IsMine)
			{
				UpdateLaserHitEffect(startPoint, lockeddirection);
				CutObjects();
			}
		}
		else if (visibleline.enabled)
		{
			line.enabled = false;
			visibleline.SetPosition(0, startPoint);
			visibleline.SetPosition(1, endPoint);
		}
		else
		{
			line.enabled = false;
			visibleline.enabled = false;
		}
	}

	public void TriggerVisualEffects()
	{
		UpdateLaserHitEffect(Gun2.position, GetFireDirection());

		if (DynamicParticles.instance != null)
		{
			DynamicParticles.instance.PlayBulletHit(10000f, transform, currentHitInfo, Color.red);
		}
	}

	void UpdateLaserHitEffect(Vector3 start, Vector3 dir)
	{
		Camera cam = Camera.main;
		if (cam == null) return;

		Vector3 finalPoint = GetLaserCameraLimit(cam, start, dir);
		currentHitInfo = new HitInfo { point = finalPoint, normal = -dir };
	}

	Vector3 GetLaserCameraLimit(Camera cam, Vector3 start, Vector3 dir)
	{
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
		float minDist = float.MaxValue;
		Vector3 bestPoint = start + dir * laserLength;

		foreach (var plane in planes)
		{
			if (plane.Raycast(new Ray(start, dir), out float enter))
			{
				if (enter > 0f && enter < minDist)
				{
					minDist = enter;
					bestPoint = start + dir * enter;
				}
			}
		}
		return bestPoint;
	}

	public void ActivateLaser(Transform arma)
	{
		this.Gun2 = arma;
		alreadyRequestedThisCut.Clear();
		cutting = true;
		locked = true;
		startPoint = arma.position;
		lockeddirection = GetFireDirection();
		endPoint = startPoint + lockeddirection * laserLength;

	}

	public void SwitchOffLaser()
	{
		cutting = false;
		locked = false;
		alreadyRequestedThisCut.Clear();
		visibleline.enabled = true;
	}

	public void SetAimLaser(bool ativa)
	{
		if (visibleline != null)
			visibleline.enabled = ativa;
	}

	private Vector3 GetFireDirection()
	{
		Quaternion shootRotation = (Quaternion)typeof(Gun).InvokeMember(
		  "getShootRotation",
		  BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic,
		  null, gun, new object[] { 0, 0, 0f }
		);
		return shootRotation * Vector3.forward;
	}

	private HashSet<Player> Playersdmg = new HashSet<Player>();

	void DamagePlayerPercent(Player targetPlayer, float percent)
	{
		if (targetPlayer == null || gun == null || gun.player == null) return;

		var characterData = targetPlayer.data;
		var damagable = targetPlayer.GetComponent<HealthHandler>();
		percent = Mathf.Clamp(percent, 0.1f, 1f);
		float damageAmount = characterData.health * percent;

		if (damageAmount <= 0f) return;

		Vector2 dir = (targetPlayer.transform.position - startPoint).normalized;

		targetPlayer.GetComponent<HealthHandler>().CallTakeDamage(
	dir * damageAmount,
	targetPlayer.transform.position,
	null,
	gun.player
);
	}

	void CutObjects()
	{
		Vector2 dir = (endPoint - startPoint).normalized;
		float dist = Vector3.Distance(startPoint, endPoint);

		foreach (var ropeGen in RopeColliderGenerator.AllRopes)
		{
			if (ropeGen == null) continue;
			if (ropeGen.CheckLaserHit(startPoint, dir, dist, out _))
			{
				var marker = ropeGen.GetComponent<RuntimeMarker>() ?? ropeGen.GetComponentInParent<RuntimeMarker>();
				if (marker != null && cutterProxy != null)
				{
					if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
						cutterProxy.photonView.RPC("RPC_SyncRopeCut", RpcTarget.All, marker.RuntimeID);
					else
						cutterProxy.photonView.RPC("RPC_RequestRopeCutOnMaster", RpcTarget.MasterClient, marker.RuntimeID);
				}
			}
		}

		RaycastHit2D[] hits = Physics2D.RaycastAll(startPoint, dir, dist, cutLayers);

		float currentPercent = 0.9f;
		var ownerEffects = gun.player.GetComponent<BlockSpawnerEffects>();

		foreach (var hit in hits)
		{
			if (hit.collider == null || hit.collider.isTrigger) continue;
			GameObject target = hit.collider.gameObject;

			if (target.layer == LayerMask.NameToLayer("Player"))
			{
				if (ownerEffects != null && ownerEffects.LaserDoDmg)
				{
					var targetPlayer = target.GetComponentInParent<Player>();
					if (targetPlayer != null && targetPlayer != gun.player && !Playersdmg.Contains(targetPlayer))
					{
						Playersdmg.Add(targetPlayer);
						DamagePlayerPercent(targetPlayer, currentPercent);
					}
				}
				currentPercent = Mathf.Max(0.1f, currentPercent - 0.1f);
				continue;
			}

			if (!alreadyRequestedThisCut.Contains(target))
			{
				var pd = target.GetComponent<PieceData>();
				if (pd != null && Time.time - pd.createdTime < 0.2f) continue;

				if (cutterProxy != null)
				{
					alreadyRequestedThisCut.Add(target);
					cutterProxy.RequestCut(target, startPoint, endPoint);

					currentPercent = Mathf.Max(0.1f, currentPercent - 0.1f);
				}
			}
		}
	}
}