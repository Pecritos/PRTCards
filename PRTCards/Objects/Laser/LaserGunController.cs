using PRT;
using PRT.Core;
using PRT.Objects.Laser;
using SoundImplementation;
using System.Collections;
using UnityEngine;

public class LaserGunController : MonoBehaviour
{
    public Gun gun;
    private LaserCutter2D laserCutter;

    public Coroutine laserCoroutine;

    private bool ActiveLaser = false;

    private float sinceAttackLaser = 999f;
    private float sinceAttackNormal = 999f;

    private float attackSpeedOriginal;
    private bool dontAllowAutoFireOriginal;
    private float spreadOriginal;

    public float attackSpeedLaser = 5f;
    public bool LoadingLaser = false;

    private AudioSource chargeSource;
    private AudioSource fireSource;

    private static AudioClip laserChargeClip;
    private static AudioClip laserFireClip;

    void Awake()
    {
        SetupAudio();
        StartCoroutine(InitLaser());
    }

    void Update()
    {
        if (gun == null || gun.player == null) return;

        float dt = Time.deltaTime;

        if (ActiveLaser)
            sinceAttackLaser += dt;
        else
            sinceAttackNormal += dt;

        var actions = gun.player.data.playerActions;
        if (actions == null) return;

        if (actions.GetAdditionalData().switchWeapon.WasPressed)
        {
            ActiveLaser = !ActiveLaser;

            if (ActiveLaser)
            {
                sinceAttackNormal = gun.sinceAttack;

                gun.sinceAttack = sinceAttackLaser;

                attackSpeedOriginal = gun.attackSpeed;
                dontAllowAutoFireOriginal = gun.dontAllowAutoFire;
                spreadOriginal = gun.spread;

                gun.attackSpeed = attackSpeedLaser;
                gun.dontAllowAutoFire = true;
                gun.spread = 0f;
            }
            else
            {
                sinceAttackLaser = gun.sinceAttack;

                gun.sinceAttack = sinceAttackNormal;

                CancelLaser();

                gun.attackSpeed = attackSpeedOriginal;
                gun.dontAllowAutoFire = dontAllowAutoFireOriginal;
                gun.spread = spreadOriginal;
            }

            if (laserCutter != null)
                laserCutter.SetAimLaser(ActiveLaser);
        }

        gun.GetAdditionalData().canShoot = !ActiveLaser;
    }

    IEnumerator InitLaser()
    {
        yield return null;

        laserCutter = gun.GetComponentInChildren<LaserCutter2D>();
        if (laserCutter == null)
        {
            var laserGO = LaserLoader.SpawnLaser(Vector3.zero, Quaternion.identity);
            laserGO.transform.SetParent(gun.transform);
            laserGO.transform.localPosition = Vector3.zero;
            laserGO.transform.localRotation = Quaternion.identity;
            laserCutter = laserGO.GetComponent<LaserCutter2D>();
        }

        laserCutter.SwitchOffLaser();
        laserCutter.SetAimLaser(false);
    }

    void SetupAudio()
    {

        chargeSource = gameObject.AddComponent<AudioSource>();
        chargeSource.volume = 0.05f;
        chargeSource.playOnAwake = false;
        fireSource = gameObject.AddComponent<AudioSource>();
        fireSource.volume = 0.055f;
        fireSource.playOnAwake = false;
        if (laserChargeClip == null)
            laserChargeClip = Assets.Bundle.LoadAsset<AudioClip>("laser_charge_loop");

        if (laserFireClip == null)
            laserFireClip = Assets.Bundle.LoadAsset<AudioClip>("laserrealease");

        chargeSource.clip = laserChargeClip;
        fireSource.clip = laserFireClip;

        var sfxGroups = SoundVolumeManager.Instance.audioMixer.FindMatchingGroups("SFX");
        if (sfxGroups.Length > 0)
        {
            chargeSource.outputAudioMixerGroup = sfxGroups[0];
            fireSource.outputAudioMixerGroup = sfxGroups[0];
        }
        else
        {
        }
    }


    public void PlayChargeSound()
    {
        if (!chargeSource.isPlaying)
            chargeSource.Play();
    }

    public void StopChargeSound()
    {
        if (chargeSource.isPlaying)
            chargeSource.Stop();
    }

    public void PlayFireSound()
    {
        fireSource.PlayOneShot(fireSource.clip);
    }

    public void CancelLaser()
    {
        if (laserCoroutine != null)
        {
            gun.StopCoroutine(laserCoroutine);
            laserCoroutine = null;
        }

        LoadingLaser = false;
        StopChargeSound();

        if (laserCutter != null)
            laserCutter.SwitchOffLaser();
    }

    public bool IsLaserActive() => ActiveLaser;

    public bool IsWaitingForCooldown()
    {
        return sinceAttackLaser < (1f / attackSpeedLaser);
    }

    public void ResetLaserCooldown()
    {
        sinceAttackLaser = 0f;
    }

    public LaserCutter2D GetLaserCutter() => laserCutter;
}
