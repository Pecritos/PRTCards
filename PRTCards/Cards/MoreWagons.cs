using UnboundLib.Cards;
using UnityEngine;

namespace PRT.Cards
{
    public class MoreWagons : CustomCard
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

            effects.wagons += 1f;
        }

        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data,
            HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            var effects = player.gameObject.GetComponent<BlockSpawnerEffects>();
            if (effects != null)
            {
                effects.wagons -= 1f;
                if (effects.wagons < 0f)
                    effects.wagons = 0f;
            }
        }

        protected override string GetTitle() => "More Wagons";

        protected override string GetDescription() => "Adds extra wagons to your train";

        protected override GameObject GetCardArt() => Assets.ComboioCard;

        protected override CardInfo.Rarity GetRarity() => CardInfo.Rarity.Uncommon;

        protected override CardInfoStat[] GetStats() => new[]
        {
            new CardInfoStat()
            {
                positive = true,
                stat = "Extra wagons",
                amount = "+1"
            }
        };

        protected override CardThemeColor.CardThemeColorType GetTheme() =>
            CardThemeColor.CardThemeColorType.TechWhite;

        public override string GetModName() => "PRT";
    }
}
