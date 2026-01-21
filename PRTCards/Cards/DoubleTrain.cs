using UnboundLib.Cards;
using UnityEngine;

namespace PRT.Cards
{
    public class DoubleTrain : CustomCard
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

            effects.DoubleTrain = true;
        }

        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data,
            HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            var effects = player.gameObject.GetComponent<BlockSpawnerEffects>();
            if (effects != null)
            {
                effects.DoubleTrain = false;
            }
        }

        protected override string GetTitle() => "Double Train";

        protected override string GetDescription() =>
            "Your train or rather, your trains really want to run someone over...";

        protected override GameObject GetCardArt() => Assets.DoubleTrainCard;

        protected override CardInfo.Rarity GetRarity() => CardInfo.Rarity.Rare;

        protected override CardInfoStat[] GetStats() => new[]
        {
            new CardInfoStat()
            {
                positive = true,
                stat = "Trains",
                amount = "2"
            }
        };

        protected override CardThemeColor.CardThemeColorType GetTheme() =>
            CardThemeColor.CardThemeColorType.DestructiveRed;

        public override string GetModName() => "PRT";
    }
}
