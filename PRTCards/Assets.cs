using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PRT
{
    internal class Assets
    {
        public static readonly AssetBundle Bundle = Jotunn.Utils.AssetUtils.LoadAssetBundleFromResources("PRTbundle", typeof(PRT).Assembly);

        public static GameObject TNTLauncherCard = Bundle.LoadAsset<GameObject>("C_TNTLauncher");
        public static GameObject TNTBoostCard = Bundle.LoadAsset<GameObject>("C_TNTBoost");
        public static GameObject TNTStormCard = Bundle.LoadAsset<GameObject>("C_TNTStorm");
        public static GameObject ComboioCard = Bundle.LoadAsset<GameObject>("C_Comboio");
        public static GameObject QuickCutCard = Bundle.LoadAsset<GameObject>("C_QuickCut");
        public static GameObject NowItHurtsCard = Bundle.LoadAsset<GameObject>("C_NowItHurts");
        public static GameObject IlikeTrainsCard = Bundle.LoadAsset<GameObject>("C_IlikeTrains");
        public static GameObject BigTrainCard = Bundle.LoadAsset<GameObject>("C_BigTrain");
        public static GameObject FastWheelsCard = Bundle.LoadAsset<GameObject>("C_FastWheels");
        public static GameObject LavaTrainCard = Bundle.LoadAsset<GameObject>("C_LavaTrain");
        public static GameObject GodOfTrainCard = Bundle.LoadAsset<GameObject>("C_GodOfTrain");
        public static GameObject BoomerangTrainCard = Bundle.LoadAsset<GameObject>("C_BoomerangTrain");
        public static GameObject DoubleTrainCard = Bundle.LoadAsset<GameObject>("C_DoubleTrain");
    }
}
