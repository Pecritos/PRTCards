using UnityEngine;

namespace PRT.Objects.TNT
{
    public static class TNTSpawner
    {
        private static GameObject tntPrefab;
        private static AssetBundle bundle;

        public static AssetBundle GetBundle()
        {
            if (bundle == null)
            {
                bundle = Assets.Bundle;
            }
            return bundle;
        }

        public static GameObject GetPrefab()
        {
            if (tntPrefab == null) LoadPrefab();
            return tntPrefab;
        }

        public static void LoadPrefab()
        {
            if (tntPrefab != null) return;

                        AssetBundle bundle = GetBundle();
            if (bundle == null)
            {
                return;
            }

            tntPrefab = bundle.LoadAsset<GameObject>("TNT");
            if (tntPrefab == null)
            {
            }
        }

        public static void SpawnTNT(GameObject owner, Player player)
        {
            LoadPrefab();
            if (tntPrefab == null)
            {
                return;
            }

            float zDist = Mathf.Abs(Camera.main.transform.position.z - owner.transform.position.z);
            Vector3 topRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, zDist));
            float cameraTop = topRight.y;

            Vector3 spawnPos = new Vector3(
                Camera.main.ViewportToWorldPoint(new Vector3(0, 0.5f, zDist)).x - 5f,
                cameraTop + 1f,
                0f
            );

            GameObject tntInstance = Object.Instantiate(tntPrefab, spawnPos, Quaternion.identity);
            var animScript = tntInstance.GetComponent<TNTScript>() ?? tntInstance.AddComponent<TNTScript>();
            animScript.player_spawner = player;

            var effects = owner.GetComponent<BlockSpawnerEffects>();
            int tntCount = effects ? Mathf.RoundToInt(effects.numberOfTNTs) : 10;
            float explodeTime = effects ? effects.timeOfExplosion : 3f;
            float tntScale = effects ? effects.TNTscale : 1f;

            animScript.tntScale = tntScale;
            animScript.StartTNT(tntCount, explodeTime, true);
        }
    }
}