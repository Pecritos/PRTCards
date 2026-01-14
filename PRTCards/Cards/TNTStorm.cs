using UnboundLib.Cards;
using UnityEngine;

namespace PRT.Cards
{
    public class TNTStorm : CustomCard
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

            effects.numberOfTNTs += 30;
            effects.timeOfExplosion = 2.5f;
        }

        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data,
            HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            var effects = player.gameObject.GetComponent<BlockSpawnerEffects>();
            if (effects != null)
            {
                effects.numberOfTNTs -= 30;
                effects.timeOfExplosion = 1f;
            }
        }

        protected override string GetTitle() => "TNT Storm";

        protected override string GetDescription() =>
            "A massive amount of TNT, but they take longer to explode";

        protected override GameObject GetCardArt() => Assets.TNTStormCard;

        protected override CardInfo.Rarity GetRarity() => CardInfo.Rarity.Rare;

        protected override CardInfoStat[] GetStats() => new[]
        {
            new CardInfoStat()
            {
                positive = true,
                stat = "TNT thrown",
                amount = "+30"
            },
            new CardInfoStat()
            {
                positive = false,
                stat = "Time to explosion",
                amount = "2.5s"
            }
        };

        protected override CardThemeColor.CardThemeColorType GetTheme() =>
            CardThemeColor.CardThemeColorType.DestructiveRed;

        public override string GetModName() => "PRT";
    }
}
