using UnityEngine;

namespace PRT.Core
{
    public static class PRTAssetLoader
    {
        private static AssetBundle bundle;

        public static AssetBundle Bundle
        {
            get
            {
                if (bundle == null)
                {
                    bundle = Assets.Bundle;
                }
                return bundle;
            }
        }

        public static bool IsLoaded => bundle != null;
    }
}
