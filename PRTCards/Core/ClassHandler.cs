using System.Collections;
using ClassesManagerReborn;
using UnityEngine;
using PRT.Cards;

namespace PRT.Core
{
	internal class ClassHandler : ClassesManagerReborn.ClassHandler
	{
		internal static string name = "Train";

		public override IEnumerator Init()
		{
			while (ILikeTrains.card == null || BigTrain.card == null || FastWheels.card == null || MoreWagons.card == null || LavaTrain.card == null
	|| TNTLauncher.card == null || TNTRain.card == null || TNTStorm.card == null)
				yield return null;

			ClassesRegistry.Register(ILikeTrains.card, (CardType)1, 0);
			ClassesRegistry.Register(ILikeLasers.card, (CardType)1, 0);
			ClassesRegistry.Register(BigTrain.card, (CardType)16, ILikeTrains.card, 0);
			ClassesRegistry.Register(FastWheels.card, (CardType)16, ILikeTrains.card, 0);
			ClassesRegistry.Register(MoreWagons.card, (CardType)16, ILikeTrains.card, 0);
			ClassesRegistry.Register(LavaTrain.card, (CardType)16, ILikeTrains.card, 0);
			ClassesRegistry.Register(GodOfTrains.card, (CardType)16, DoubleTrain.card, 0);
			ClassesRegistry.Register(DoubleTrain.card, (CardType)16, ILikeTrains.card, 0);
			ClassesRegistry.Register(BoomerangTrain.card, (CardType)16, ILikeTrains.card, 0);
			ClassesRegistry.Register(NowItHurts.card, (CardType)16, ILikeLasers.card, 0);

			ClassesRegistry.Register(TNTRain.card, (CardType)16, TNTLauncher.card, 0);
		}
	}
}
