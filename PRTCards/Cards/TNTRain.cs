using UnboundLib.Cards;
using UnityEngine;

namespace PRT.Cards
{
    public class TNTRain : CustomCard
    {
        internal static CardInfo card;

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats, CharacterStatModifiers statModifiers, Block block)
        {
            card = cardInfo;
        }

        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data,
            HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            var effects = player.gameObject.GetComponent<BlockSpawnerEffects>();
            if (effects == null) effects = player.gameObject.AddComponent<BlockSpawnerEffects>();

            effects.numberOfTNTs += 5;
            effects.timeOfExplosion -= 0.5f;
            if (effects.timeOfExplosion < 0.5f)
                effects.timeOfExplosion = 0.5f;
        }

        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data,
            HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            var effects = player.gameObject.GetComponent<BlockSpawnerEffects>();
            if (effects != null)
            {
                effects.numberOfTNTs -= 5;
                effects.timeOfExplosion += 0.5f;
            }
        }

        protected override string GetTitle() => "TNT Rain";

        protected override string GetDescription() => "More TNT, and faster explosions.";

        protected override GameObject GetCardArt() => Assets.TNTBoostCard;

        protected override CardInfo.Rarity GetRarity() => CardInfo.Rarity.Uncommon;

        protected override CardInfoStat[] GetStats() => new[]
        {
            new CardInfoStat()
            {
                positive = true,
                stat = "TNT thrown",
                amount = "+5"
            },
            new CardInfoStat()
            {
                positive = true,
                stat = "Time to explosion",
                amount = "-0.5s"
            }
        };

        protected override CardThemeColor.CardThemeColorType GetTheme() =>
            CardThemeColor.CardThemeColorType.DestructiveRed;

        public override string GetModName() => "PRT";
    }
}
