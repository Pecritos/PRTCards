using PRT;
using PRT.Core;
using PRT.Objects.Laser;
using SoundImplementation;
using System.Collections;
using UnityEngine;
using Photon.Pun;

public class LaserGunController : MonoBehaviour
{
	public Gun gun;
	private LaserCutter2D laserCutter;
	public Coroutine laserCoroutine;

	private bool ActiveLaser = false;
	private float sinceAttackLaser = 999f;
	private float sinceAttackNormal = 999f;

	private float attackSpeedOriginal = -1f;
	private bool dontAllowAutoFireOriginal;
	private float spreadOriginal;

	public float attackSpeedLaser = 5f;
	public bool LoadingLaser = false;

	private AudioSource chargeSource;
	private AudioSource fireSource;

	private static AudioClip laserChargeClip;
	private static AudioClip laserFireClip;

	private PhotonView PlayerView => GetComponent<PhotonView>();

	void Awake()
	{
		SetupAudio();
	}

	void Start()
	{
		StartCoroutine(InitLaser());
	}

	void Update()
	{
		if (gun == null || gun.player == null || PlayerView == null) return;
		if (!PlayerView.IsMine) return;

		float dt = Time.deltaTime;
		if (ActiveLaser) sinceAttackLaser += dt;
		else sinceAttackNormal += dt;

		var actions = gun.player.data.playerActions;
		if (actions != null && actions.GetAdditionalData().switchWeapon.WasPressed)
		{
			PlayerView.RPC("RPC_ToggleLaserMode", RpcTarget.All, !ActiveLaser);
		}
	}

	[PunRPC]
	private void RPC_ToggleLaserMode(bool activate)
	{
		ActiveLaser = activate;
		if (ActiveLaser)
		{
			if (attackSpeedOriginal < 0)
			{
				attackSpeedOriginal = gun.attackSpeed;
				dontAllowAutoFireOriginal = gun.dontAllowAutoFire;
				spreadOriginal = gun.spread;
			}
			sinceAttackNormal = gun.sinceAttack;
			gun.sinceAttack = sinceAttackLaser;
			gun.attackSpeed = attackSpeedLaser;
			gun.dontAllowAutoFire = true;
			gun.spread = 0f;
		}
		else
		{
			sinceAttackLaser = gun.sinceAttack;
			gun.sinceAttack = sinceAttackNormal;
			CancelLaser();
			if (attackSpeedOriginal >= 0)
			{
				gun.attackSpeed = attackSpeedOriginal;
				gun.dontAllowAutoFire = dontAllowAutoFireOriginal;
				gun.spread = spreadOriginal;
			}
		}

		if (laserCutter != null) laserCutter.SetAimLaser(ActiveLaser);
	}

	public void NetworkPlayCharge() => PlayerView.RPC("RPC_LaserCharge", RpcTarget.All, true);
	public void NetworkStopCharge() => PlayerView.RPC("RPC_LaserCharge", RpcTarget.All, false);
	public void NetworkPlayFire() => PlayerView.RPC("RPC_LaserFire", RpcTarget.All);

	[PunRPC]
	private void RPC_LaserCharge(bool start) { if (start) PlayChargeSound(); else StopChargeSound(); }

	[PunRPC]
	private void RPC_LaserFire()
	{
		PlayFireSound();

		if (laserCutter != null)
		{
			laserCutter.TriggerVisualEffects();

			Vector3 dir = gun.shootPosition.forward;
			GamefeelManager.GameFeel(dir * 30f);
		}
	}

	IEnumerator InitLaser()
	{
		while (gun == null) yield return null;
		yield return new WaitForSeconds(0.1f);
		laserCutter = gun.GetComponentInChildren<LaserCutter2D>();
		if (laserCutter != null)
		{
			laserCutter.SwitchOffLaser();
			laserCutter.SetAimLaser(ActiveLaser);
		}
	}

	void SetupAudio()
	{
		chargeSource = gameObject.AddComponent<AudioSource>();
		chargeSource.volume = 0.05f;
		chargeSource.playOnAwake = false;
		chargeSource.loop = false;
		fireSource = gameObject.AddComponent<AudioSource>();
		fireSource.volume = 0.055f;
		fireSource.playOnAwake = false;
		fireSource.loop = false;

		if (laserChargeClip == null) laserChargeClip = Assets.Bundle.LoadAsset<AudioClip>("laser_charge_loop");
		if (laserFireClip == null) laserFireClip = Assets.Bundle.LoadAsset<AudioClip>("laserrealease");

		chargeSource.clip = laserChargeClip;
		fireSource.clip = laserFireClip;

		var groups = SoundVolumeManager.Instance.audioMixer.FindMatchingGroups("SFX");
		if (groups.Length > 0) { chargeSource.outputAudioMixerGroup = groups[0]; fireSource.outputAudioMixerGroup = groups[0]; }
	}

	public void PlayChargeSound() { if (!chargeSource.isPlaying) chargeSource.Play(); }
	public void StopChargeSound() { if (chargeSource.isPlaying) chargeSource.Stop(); }
	public void PlayFireSound() { if (fireSource.clip != null) fireSource.PlayOneShot(fireSource.clip); }

	public void CancelLaser()
	{
		if (laserCoroutine != null) { gun.StopCoroutine(laserCoroutine); laserCoroutine = null; }
		LoadingLaser = false;
		StopChargeSound();
		if (laserCutter != null) laserCutter.SwitchOffLaser();
	}

	public bool IsLaserActive() => ActiveLaser;
	public bool IsWaitingForCooldown() => sinceAttackLaser < (1f / attackSpeedLaser);
	public void ResetLaserCooldown() => sinceAttackLaser = 0f;
	public LaserCutter2D GetLaserCutter() => laserCutter;
}