using BepInEx;
using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using HarmonyLib;
using RarityLib;
using RarityLib.Utils;
using PRT.Cards;
using PRT.Core;
using PRT.Objects;
using System.Collections;
using TMPro;
using UnboundLib;
using UnboundLib.Cards;
using UnboundLib.Utils.UI;
using UnityEngine;
using UnityEngine.UI;

namespace PRT
{
	[BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
	[BepInDependency("pykess.rounds.plugins.cardchoicespawnuniquecardpatch", BepInDependency.DependencyFlags.HardDependency)]
	[BepInPlugin(ModId, ModName, Version)]
	[BepInProcess("Rounds.exe")]
	public class PRT : BaseUnityPlugin
	{
		private const string ModId = "com.seunome.PRT";
		private const string ModName = "PRT";
		public const string Version = "1.0.0";

		public static PRT instance { get; private set; }

		void Awake()
		{
			instance = this;

			var harmony = new Harmony(ModId);
			harmony.PatchAll();

			RarityUtils.AddRarity(
				"Legendary",
				0.025f,
				new Color(112f / 255f, 209f / 255f, 244f / 255f),
				new Color(0.7f, 0.7f, 0f)
			);
		}

		void Start()
		{
			CustomCard.BuildCard<ILikeTrains>();
			CustomCard.BuildCard<BigTrain>();
			CustomCard.BuildCard<FastWheels>();
			CustomCard.BuildCard<MoreWagons>();
			CustomCard.BuildCard<LavaTrain>();
			CustomCard.BuildCard<TNTLauncher>();
			CustomCard.BuildCard<TNTRain>();
			CustomCard.BuildCard<TNTStorm>();
			CustomCard.BuildCard<QuickCut>();
			CustomCard.BuildCard<NowItHurts>();
			CustomCard.BuildCard<BoomerangTrain>();
			CustomCard.BuildCard<DoubleTrain>();
			CustomCard.BuildCard<GodOfTrains>();

			StartCoroutine(new ClassHandler().Init());

			if (GameObject.Find("RopeAutoConverter_Global") == null)
			{
				GameObject obj = new GameObject("RopeAutoConverter_Global");
				obj.AddComponent<RopeAutoConverter>();
				DontDestroyOnLoad(obj);
			}

			if (FindObjectOfType<MapSceneRuntimeIDManager>() == null)
			{
				var go = new GameObject("MapSceneRuntimeIDManager");
				var manager = go.AddComponent<MapSceneRuntimeIDManager>();
				DontDestroyOnLoad(go);
			}

			Unbound.RegisterCredits(
				ModName,
				new[] { "Pecritos" },
				new[] { "github" },
				new[] { "https://github.com/Pecritos/PRTCards" }
			);

		}
	}
}
