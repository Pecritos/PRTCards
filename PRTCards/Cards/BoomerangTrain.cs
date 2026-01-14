using UnboundLib.Cards;
using UnityEngine;

namespace PRT.Cards
{
    public class BoomerangTrain : CustomCard
    {
        internal static CardInfo card;

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats,
            CharacterStatModifiers statModifiers, Block block)
        {
            card = cardInfo;
        }

        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data,
            HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            var effects = player.gameObject.GetComponent<BlockSpawnerEffects>();
            if (effects == null) effects = player.gameObject.AddComponent<BlockSpawnerEffects>();

            effects.boomerang = true;
        }

        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data,
            HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            var effects = player.gameObject.GetComponent<BlockSpawnerEffects>();
            if (effects != null)
            {
                effects.boomerang = false;
            }
        }

        protected override string GetTitle() => "Boomerang Train";

        protected override string GetDescription() =>
            "Who said a train can't go in reverse?";

        protected override GameObject GetCardArt() => Assets.BoomerangTrainCard;

        protected override CardInfo.Rarity GetRarity() => CardInfo.Rarity.Rare;

        protected override CardInfoStat[] GetStats() => null;

        protected override CardThemeColor.CardThemeColorType GetTheme() =>
            CardThemeColor.CardThemeColorType.TechWhite;

        public override string GetModName() => "PRT";
    }
}
