using System.IO;
using System.Reflection;
using UnityEngine;

namespace PRT.Objects.Laser
{
    public static class LaserLoader
    {
        private static GameObject laserPrefab;
        private static AssetBundle bundle;

                                public static void LoadBundle()
        {
            if (bundle != null) return;
            bundle = Assets.Bundle;
        }

                                public static void LoadPrefab()
        {
            LoadBundle();

            if (bundle != null)
            {
                if (laserPrefab == null)
                {
                                        laserPrefab = bundle.LoadAsset<GameObject>("Laser");

                                        if (laserPrefab != null && laserPrefab.GetComponent<LaserCutter2D>() == null)
                    {
                        laserPrefab.AddComponent<LaserCutter2D>();
                    }
                }
            }
            else
            {
            }
        }

                                public static GameObject SpawnLaser(Vector3 position, Quaternion rotation)
        {
            LoadPrefab();

            if (laserPrefab == null)
            {
                return null;
            }

            GameObject laserInstance = Object.Instantiate(laserPrefab, position, rotation);
            return laserInstance;
        }
    }
}