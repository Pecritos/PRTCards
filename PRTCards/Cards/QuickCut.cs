using HarmonyLib;
using Photon.Pun;
using PRT.Core;
using PRT.Objects.Laser;
using System.Collections;
using UnboundLib.Cards;
using UnityEngine;

namespace PRT.Cards
{
    public class QuickCut : CustomCard
    {
        internal static CardInfo card;
        protected override string GetTitle() => "Quick Cut";
        protected override string GetDescription() => "Press C on the keyboard (or <- on a controller) to switch your weapon to a laser that cuts map objects (does not affect players).";

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new[] { TrainClass.TrainCategory };
            card = cardInfo;
        }

        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
                        LaserNetworkActionProxy proxy = player.gameObject.GetComponent<LaserNetworkActionProxy>() ?? player.gameObject.AddComponent<LaserNetworkActionProxy>();

                        var laserCtrl = player.gameObject.GetComponent<LaserGunController>() ?? player.gameObject.AddComponent<LaserGunController>();
            laserCtrl.gun = gun;

                        player.data.view.RefreshRpcMonoBehaviourCache();

                        var laser = gun.GetComponentInChildren<LaserCutter2D>();
            if (laser == null)
            {
                var laserGO = LaserLoader.SpawnLaser(Vector3.zero, Quaternion.identity);
                if (laserGO != null)
                {
                    laserGO.transform.SetParent(gun.transform);
                    laserGO.transform.localPosition = Vector3.zero;
                    laserGO.transform.localRotation = Quaternion.identity;
                    laser = laserGO.GetComponent<LaserCutter2D>();
                }
            }

            if (laser != null) laser.cutterProxy = proxy;
        }

        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data, HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            var laser = gun.GetComponentInChildren<LaserCutter2D>();
            if (laser != null) Object.Destroy(laser.gameObject);

            var laserCtrl = player.GetComponent<LaserGunController>();
            if (laserCtrl != null) Object.Destroy(laserCtrl);
        }

        protected override CardInfoStat[] GetStats() => new CardInfoStat[] { new CardInfoStat { amount = "5s", positive = false, stat = "Laser ATK Speed" } };
        protected override CardInfo.Rarity GetRarity() => CardInfo.Rarity.Rare;
        protected override GameObject GetCardArt() => Assets.QuickCutCard;
        protected override CardThemeColor.CardThemeColorType GetTheme() => CardThemeColor.CardThemeColorType.DestructiveRed;
        public override string GetModName() => "PRT";
    }

    [HarmonyPatch(typeof(Gun), "Attack")]
    class Gun_Attack_Patch
    {
        static bool Prefix(Gun __instance, ref bool __result)
        {
                        if (__instance.player == null) return true;
            var laserCtrl = __instance.player.GetComponent<LaserGunController>();

            if (laserCtrl == null || !laserCtrl.IsLaserActive()) return true;

            if (laserCtrl.LoadingLaser || laserCtrl.IsWaitingForCooldown())
            {
                __result = false;
                return false;
            }

            laserCtrl.LoadingLaser = true;
            laserCtrl.NetworkPlayCharge();
            laserCtrl.laserCoroutine = __instance.StartCoroutine(ChargeAndFire(__instance, laserCtrl));

            __result = true;
            return false;
        }

        static IEnumerator ChargeAndFire(Gun gun, LaserGunController ctrl)
        {
            yield return new WaitForSeconds(2f);
            if (!ctrl.IsLaserActive())
            {
                ctrl.LoadingLaser = false;
                ctrl.NetworkStopCharge();
                yield break;
            }

            ctrl.ResetLaserCooldown();
            gun.sinceAttack = 0f;
            var laser = ctrl.GetLaserCutter();
            if (laser != null)
            {
                ctrl.NetworkStopCharge();
                ctrl.NetworkPlayFire();
                laser.ActivateLaser(gun.shootPosition);
                gun.StartCoroutine(TurnOffAfter(laser, 0.1f));
            }
            ctrl.LoadingLaser = false;
        }

        static IEnumerator TurnOffAfter(LaserCutter2D laser, float t)
        {
            yield return new WaitForSeconds(t);
            if (laser != null) laser.SwitchOffLaser();
        }
    }
}