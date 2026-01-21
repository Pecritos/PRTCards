using Photon.Pun;
using UnityEngine;
using UnboundLib.Cards;
using System;
using UnityEngine.SceneManagement;
using PRT.Objects.TNT;

namespace PRT.Cards
{
    public class TNTLauncher : CustomCard
    {
        internal static CardInfo card;
        private Action<BlockTrigger.BlockTriggerType> blockCallback;

        protected override string GetTitle() => "TNT Launcher";

        protected override string GetDescription() =>
            "Blocking launches TNT from the sky that explodes and causes chaos!";

        public override void SetupCard(CardInfo cardInfo, Gun gun, ApplyCardStats cardStats,
            CharacterStatModifiers statModifiers)
        {
            cardInfo.allowMultiple = false;
            card = cardInfo;

            block.cooldown += 0.5f;
        }

        public override void OnAddCard(Player player, Gun gun, GunAmmo gunAmmo, CharacterData data,
            HealthHandler health, Gravity gravity, Block block, CharacterStatModifiers characterStats)
        {
            if (ExplosionPool.Instance == null)
            {
                GameObject poolGO = new GameObject("ExplosionPoolManager");
                poolGO.AddComponent<ExplosionPool>();
            }

                        var effects = player.gameObject.GetComponent<BlockSpawnerEffects>();
            if (effects == null) effects = player.gameObject.AddComponent<BlockSpawnerEffects>();

                        var tntproxy = player.gameObject.GetComponent<TNTNetworkProxy>();
            if (tntproxy == null) tntproxy = player.gameObject.AddComponent<TNTNetworkProxy>();

            blockCallback = (BlockTrigger.BlockTriggerType triggerType) =>
            {
                if (!PhotonNetwork.IsMasterClient) return;

                var cooldown = player.gameObject.GetComponent<TNTCooldown>();
                if (cooldown == null)
                {
                    cooldown = player.gameObject.AddComponent<TNTCooldown>();
                }

                if (!cooldown.CanUse()) return;

                cooldown.Trigger();

                TNTSpawner.SpawnTNT(player.gameObject, player);
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
            new CardInfoStat() { amount = "2s", positive = false, stat = "TNT ability cooldown" },
            new CardInfoStat() { amount = "10", positive = true, stat = "TNT thrown" }
        };

        protected override CardInfo.Rarity GetRarity() => CardInfo.Rarity.Rare;

        protected override GameObject GetCardArt() => Assets.TNTLauncherCard;

        protected override CardThemeColor.CardThemeColorType GetTheme() =>
            CardThemeColor.CardThemeColorType.DestructiveRed;

        public override string GetModName() => "PRT";
    }

    public class TNTCooldown : MonoBehaviour
    {
        private float cooldownTime = 2f;
        private float lastUse = -100f;

        public bool CanUse()
        {
            return Time.time - lastUse > cooldownTime;
        }

        public void Trigger()
        {
            lastUse = Time.time;
        }
    }
}
