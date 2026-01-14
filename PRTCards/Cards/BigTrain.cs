using UnboundLib.Cards;
using UnityEngine;

namespace PRT.Cards
{
    public class BigTrain : CustomCard
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

            effects.blockScale += 0.6f;
            effects.blockSpeed -= 40f;
        }

        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data,
            HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            var effects = player.gameObject.GetComponent<BlockSpawnerEffects>();
            if (effects != null)
            {
                effects.blockScale -= 0.6f;
                effects.blockSpeed += 40f;

                if (Mathf.Approximately(effects.blockScale, 2f) &&
                    Mathf.Approximately(effects.blockSpeed, 200f))
                {
                    Object.Destroy(effects);
                }
            }
        }

        protected override string GetTitle() => "Heavy Train";

        protected override string GetDescription() => "Your train becomes larger, but loses speed";

        protected override GameObject GetCardArt() => Assets.BigTrainCard;

        protected override CardInfo.Rarity GetRarity() => CardInfo.Rarity.Uncommon;

        protected override CardInfoStat[] GetStats() => new[]
        {
            new CardInfoStat
            {
                positive = true,
                stat = "Train size",
                amount = "Slightly larger"
            },
            new CardInfoStat
            {
                positive = false,
                stat = "Train speed",
                amount = "Slightly slower"
            }
        };

        protected override CardThemeColor.CardThemeColorType GetTheme() =>
            CardThemeColor.CardThemeColorType.DestructiveRed;

        public override string GetModName() => "PRT";
    }
}
