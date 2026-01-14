using HarmonyLib;
using InControl;
using ModdingUtils.Extensions;
using Photon.Pun;
using PRT;
using PRT.Core;
using PRT.Objects.Laser;
using PRT.Objects.Train;
using System.Collections;
using System.Reflection;
using UnboundLib.Cards;
using UnityEngine;
using RCObjects = PRT.Objects;

namespace PRT.Cards
{
    public class ILikeLasers : CustomCard
    {
        internal static CardInfo card;

        protected override string GetTitle() => "Quick Cut";

        protected override string GetDescription() =>
            "Press C on the keyboard (or <- on a controller) to switch your weapon to a laser that cuts map objects (does not affect players).";

        public override void SetupCard(
            CardInfo cardInfo,
            Gun gun,
            ApplyCardStats cardStats,
            CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new[] { TrainClass.TrainCategory };
            card = cardInfo;
        }

        public override void OnAddCard(
            Player player,
            Gun gun,
            GunAmmo gunAmmo,
            CharacterData data,
            HealthHandler health,
            Gravity gravity,
            Block block,
            CharacterStatModifiers characterStats)
        {
            var laserCtrl = gun.GetComponent<LaserGunController>();
            if (laserCtrl == null)
            {
                laserCtrl = gun.gameObject.AddComponent<LaserGunController>();
                laserCtrl.gun = gun;
            }

            var laser = gun.GetComponentInChildren<LaserCutter2D>();
            if (laser == null)
            {
                var laserGO = LaserLoader.SpawnLaser(Vector3.zero, Quaternion.identity);
                if (laserGO != null)
                {
                    laserGO.transform.SetParent(gun.transform);
                    laserGO.transform.localPosition = Vector3.zero;
                    laserGO.transform.localRotation = Quaternion.identity;
                }
            }
        }

        public override void OnRemoveCard(
            Player player,
            Gun gun,
            GunAmmo gunAmmo,
            CharacterData data,
            HealthHandler health,
            Gravity gravity,
            Block block,
            CharacterStatModifiers characterStats)
        {
            var laser = gun.GetComponentInChildren<LaserCutter2D>();
            if (laser != null)
            {
                Object.Destroy(laser.gameObject);
            }

            var laserCtrl = gun.GetComponent<LaserGunController>();
            if (laserCtrl != null)
            {
                Object.Destroy(laserCtrl);
            }
        }

        protected override CardInfoStat[] GetStats() => new CardInfoStat[]
        {
            new CardInfoStat()
            {
                amount = "5s",
                positive = false,
                stat = "Laser ATK Speed"
            }
        };

        protected override CardInfo.Rarity GetRarity() => CardInfo.Rarity.Rare;

        protected override GameObject GetCardArt() => Assets.QuickCutCard;

        protected override CardThemeColor.CardThemeColorType GetTheme() =>
            CardThemeColor.CardThemeColorType.DestructiveRed;

        public override string GetModName() => "PRT";
    }

    [HarmonyPatch(typeof(Gun), "Attack")]
    class Gun_Attack_Patch
    {
        static bool Prefix(Gun __instance, ref bool __result)
        {
            var laserCtrl = __instance.GetComponent<LaserGunController>();
            if (laserCtrl == null || !laserCtrl.IsLaserActive())
                return true;

            if (laserCtrl.LoadingLaser || laserCtrl.IsWaitingForCooldown())
            {
                __result = false;
                return false;
            }

            laserCtrl.LoadingLaser = true;
            laserCtrl.PlayChargeSound();

            laserCtrl.laserCoroutine =
                __instance.StartCoroutine(ChargeAndFire(__instance, laserCtrl));

            __result = true;
            return false;
        }

        static IEnumerator ChargeAndFire(Gun gun, LaserGunController ctrl)
        {
            yield return new WaitForSeconds(2f);

            if (!ctrl.IsLaserActive())
            {
                ctrl.LoadingLaser = false;
                ctrl.StopChargeSound();
                yield break;
            }

            ctrl.ResetLaserCooldown();
            gun.sinceAttack = 0f;

            var laser = ctrl.GetLaserCutter();
            if (laser != null)
            {
                laser.ActivateLaser(gun.shootPosition);
                ctrl.PlayFireSound();
                gun.StartCoroutine(TurnOffAfter(laser, 0.1f));
            }

            ctrl.StopChargeSound();
            ctrl.LoadingLaser = false;
        }

        static IEnumerator TurnOffAfter(LaserCutter2D laser, float t)
        {
            yield return new WaitForSeconds(t);
            if (laser != null)
                laser.SwitchOffLaser();
        }
    }
}
