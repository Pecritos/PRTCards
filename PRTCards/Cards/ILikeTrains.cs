using Photon.Pun;
using UnityEngine;
using UnboundLib.Cards;
using System;
using PRT.Core;
using PRT.Objects.Train;

namespace PRT.Cards
{
    public class ILikeTrains : CustomCard
    {
        internal static CardInfo card;
        private Action<BlockTrigger.BlockTriggerType> blockCallback;

        protected override string GetTitle() => "I Like Trains";

        protected override string GetDescription() =>
            "Blocking summons a small train that hits everyone (and unlocks special cards for your train).";

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats,
            CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
            cardInfo.categories = new[] { TrainClass.TrainCategory };
            block.cooldown += 0.5f;
            card = cardInfo;
        }

        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data,
            HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            var effects = player.gameObject.GetComponent<BlockSpawnerEffects>();
            if (effects == null) effects = player.gameObject.AddComponent<BlockSpawnerEffects>();

            blockCallback = (BlockTrigger.BlockTriggerType triggerType) =>
            {
                if (!PhotonNetwork.IsMasterClient) return;

                var cooldown = player.gameObject.GetComponent<TrainCooldown>();
                if (cooldown == null)
                {
                    cooldown = player.gameObject.AddComponent<TrainCooldown>();
                }

                if (!cooldown.CanUse()) return;

                cooldown.Trigger();

                if (effects.GodOfTrains == true)
                {
                    TrainSpawner.SpawnTrensAsterisco(player);
                }
                else
                {
                    TrainSpawner.SpawnBlock(player, TrainSpawner.TrainDirection.LeftToRight);

                    if (effects != null && effects.DoubleTrain)
                    {
                        TrainSpawner.SpawnBlock(player, TrainSpawner.TrainDirection.TopToBottom);
                    }
                }
            };

            block.BlockAction += blockCallback!;
        }

        public override void OnRemoveCard()
        {
            if (blockCallback != null)
            {
                var block = UnityEngine.Object.FindObjectOfType<Block>();
                if (block != null)
                {
                    block.BlockAction -= blockCallback;
                }
                blockCallback = null;
            }
        }

        protected override CardInfoStat[] GetStats() => new[]
        {
            new CardInfoStat() { amount = "+0.5s", positive = false, stat = "Block cooldown" },
            new CardInfoStat() { amount = "2s", positive = false, stat = "Train ability cooldown" }
        };

        protected override CardInfo.Rarity GetRarity() => CardInfo.Rarity.Rare;

        protected override GameObject GetCardArt() => Assets.IlikeTrainsCard;

        protected override CardThemeColor.CardThemeColorType GetTheme() =>
            CardThemeColor.CardThemeColorType.DestructiveRed;

        public override string GetModName() => "PRT";
    }
}
