using UnboundLib.Cards;
using UnityEngine;

namespace PRT.Cards
{
    public class FastWheels : CustomCard
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

            effects.blockSpeed += 80f;
            effects.blockScale -= 0.4f;
        }

        public override void OnRemoveCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data,
            HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            var effects = player.gameObject.GetComponent<BlockSpawnerEffects>();
            if (effects != null)
            {
                effects.blockSpeed -= 80f;
                effects.blockScale += 0.4f;

                if (Mathf.Approximately(effects.blockScale, 2f) &&
                    Mathf.Approximately(effects.blockSpeed, 200f))
                {
                    Object.Destroy(effects);
                }
            }
        }

        protected override string GetTitle() => "Bullet Train";

        protected override string GetDescription() => "Your train moves much faster";

        protected override GameObject GetCardArt() => Assets.FastWheelsCard;

        protected override CardInfo.Rarity GetRarity() => CardInfo.Rarity.Uncommon;

        protected override CardInfoStat[] GetStats() => new[]
        {
            new CardInfoStat
            {
                positive = true,
                stat = "Train speed",
                amount = "Slightly higher"
            },
            new CardInfoStat
            {
                positive = false,
                stat = "Train size",
                amount = "Slightly smaller"
            }
        };

        protected override CardThemeColor.CardThemeColorType GetTheme() =>
            CardThemeColor.CardThemeColorType.TechWhite;

        public override string GetModName() => "PRT";
    }
}
