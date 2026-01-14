using Photon.Pun;
using UnboundLib.Cards;
using UnityEngine;
using PRT;
using PRT.Core;

namespace PRT.Cards
{
    public class NowItHurts : CustomCard
    {
        internal static CardInfo card;

        protected override string GetTitle() => "Now It Hurts";

        protected override string GetDescription() =>
            "Your laser now deals 90% of the target's current health, but loses power for each object it passes through.";

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
            var effects = player.GetComponent<BlockSpawnerEffects>();
            if (effects == null)
            {
                effects = player.gameObject.AddComponent<BlockSpawnerEffects>();
            }

            effects.LaserDoDmg = true;

            var laserCtrl = gun.GetComponent<LaserGunController>();
            if (laserCtrl == null)
            {
                laserCtrl = gun.gameObject.AddComponent<LaserGunController>();
            }

            laserCtrl.attackSpeedLaser += 3f;
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
            var effects = player.GetComponent<BlockSpawnerEffects>();
            if (effects != null)
            {
                effects.LaserDoDmg = false;
            }
        }

        protected override CardInfo.Rarity GetRarity() => CardInfo.Rarity.Rare;

        protected override CardInfoStat[] GetStats() => new[]
        {
            new CardInfoStat
            {
                positive = false,
                stat = "Laser ATKSPD",
                amount = "+3s"
            },
            new CardInfoStat
            {
                positive = false,
                stat = "Per object hit",
                amount = "-10% laser damage"
            }
        };

        protected override GameObject GetCardArt() => Assets.NowItHurtsCard;

        protected override CardThemeColor.CardThemeColorType GetTheme() =>
            CardThemeColor.CardThemeColorType.DestructiveRed;

        public override string GetModName() => "PRT";
    }
}
