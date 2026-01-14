using PRT.UI;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using PRT.Core;


namespace PRT.Objects.Train
{
	public static class TrainSpawner
	{
		private static GameObject blockPrefab;
		private static GameObject vagaoPrefab;

		private static GameObject centerAsterisco;

		public enum TrainDirection
		{
			LeftToRight,
			TopToBottom
		}

		

		public static void LoadPrefab()
		{
			
			if (blockPrefab == null)
			{
				var bundle = PRTAssetLoader.Bundle;
				if (bundle == null) return;

				blockPrefab = bundle.LoadAsset<GameObject>("Tremprefab_IgnoreWater");
				vagaoPrefab = bundle.LoadAsset<GameObject>("VagaoPrefab_IgnoreWater");
			}

		}

		private static GameObject SpawnTremInterno(Player player)
		{
			LoadPrefab();
			if (blockPrefab == null) return null;

			Vector3 camLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0.5f, 0));
			Vector3 spawnPos = new Vector3(camLeft.x - 500f, player.transform.position.y, 0f);
			Quaternion spawnRot = Quaternion.identity;

			GameObject trem = Object.Instantiate(blockPrefab, spawnPos, spawnRot);

			var effects = player.GetComponent<BlockSpawnerEffects>();
			float scale = effects != null ? effects.blockScale : 2f;
			float speed = effects != null ? effects.blockSpeed : 200f;

			trem.transform.localScale = new Vector3(scale, scale, 1f);

			Rigidbody2D rb = trem.GetComponent<Rigidbody2D>();
			if (rb != null)
			{
				rb.gravityScale = 0f;
				rb.velocity = new Vector2(speed, 0f);
				rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
			}

			return trem;
		}


		public static void SpawnTrensAsterisco(Player player)
		{
			LoadPrefab();
			if (blockPrefab == null) return;

			if (centerAsterisco == null)
				centerAsterisco = new GameObject("centerAsterisco");

			Transform cen = centerAsterisco.transform;
			cen.position = player.transform.position;

			List<GameObject> trens = new List<GameObject>();

			var effects = player.GetComponent<BlockSpawnerEffects>();
			float scale = effects != null ? effects.blockScale : 2f;
			float speed = effects != null ? effects.blockSpeed : 200f;
			int vagoes = effects != null ? (int)effects.wagons : 0;
			bool lava = effects != null && effects.lava;
			bool bumerangue = effects != null && effects.boomerang;
			int quantidade = effects != null ? effects.numberofgodtrains : 8;
			float graus = effects != null ? effects.degreegodtrains : 45f;

			Vector3 camTopRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
			Vector3 camTopLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0));
			Vector3 camBottomRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0));
			Vector3 camBottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));

			float dx = Mathf.Max(Mathf.Abs(cen.position.x - camTopRight.x), Mathf.Abs(cen.position.x - camTopLeft.x),
								 Mathf.Abs(cen.position.x - camBottomRight.x), Mathf.Abs(cen.position.x - camBottomLeft.x));
			float dy = Mathf.Max(Mathf.Abs(cen.position.y - camTopRight.y), Mathf.Abs(cen.position.y - camTopLeft.y),
								 Mathf.Abs(cen.position.y - camBottomRight.y), Mathf.Abs(cen.position.y - camBottomLeft.y));

			float distancia = (dx + dy) / 2f + 200f;
			for (int i = 0; i < quantidade; i++)
			{
				float angle = i * 360f / quantidade;
				float rad = angle * Mathf.Deg2Rad;

				Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * distancia;
				Vector3 spawnPos = cen.position + offset;

				GameObject trem = Object.Instantiate(blockPrefab, spawnPos, Quaternion.identity);

				var owner = trem.GetComponent<TrainOwner>();
				if (owner == null)
					owner = trem.AddComponent<TrainOwner>();

				owner.owner = player;
				trem.transform.localScale = new Vector3(scale, scale, 1f);
				ApplyGlowToAllSprites(trem, Color.white, Color.white * 2f);

				Rigidbody2D rb = trem.GetComponent<Rigidbody2D>();
				if (rb != null)
				{
					rb.gravityScale = 0f;
					rb.velocity = Vector2.zero;
					rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
				}

				trens.Add(trem);
			}

			Collider2D[] playerCols = player.GetComponentsInChildren<Collider2D>();

			for (int i = 0; i < trens.Count; i++)
			{
				var tremA = trens[i];
				Collider2D colA = tremA.GetComponent<Collider2D>();
				if (colA == null) continue;

				for (int j = i + 1; j < trens.Count; j++)
				{
					var tremB = trens[j];
					Collider2D colB = tremB.GetComponent<Collider2D>();
					if (colB != null)
						Physics2D.IgnoreCollision(colA, colB, true);
				}

				foreach (var pCol in playerCols)
				{
					if (pCol != null)
						Physics2D.IgnoreCollision(colA, pCol, true);
				}
			}

			foreach (var trem in trens)
			{
				var auto = trem.GetComponent<TrainAutoDestroy>();
				if (auto == null)
					auto = trem.AddComponent<TrainAutoDestroy>();

				auto.bornFromAsterisco = true;
				auto.vagoes.Clear();
				auto.bumerangueAtivo = bumerangue;

				if (trem.GetComponent<TrainObject>() == null)
					trem.AddComponent<TrainObject>();

				for (int i = 0; i < vagoes; i++)
				{
					GameObject vagao = Object.Instantiate(vagaoPrefab);
					vagao.transform.SetParent(trem.transform, false);
					ApplyGlowToAllSprites(vagao, Color.white, Color.white * 2f);
					auto.vagoes.Add(vagao);

					Collider2D colV = vagao.GetComponent<Collider2D>();
					if (colV != null)
					{
						foreach (var pCol in playerCols)
						{
							if (pCol != null)
								Physics2D.IgnoreCollision(colV, pCol, true);
						}
					}

					if (lava)
					{
						var lavaVagao = vagao.AddComponent<WWMO.MonoBehaviours.BoxTouchingLava_Mono>();
						lavaVagao.heatPercent = 1f;
						typeof(WWMO.MonoBehaviours.BoxTouchingLava_Mono)
							.GetField("heatDuration", BindingFlags.NonPublic | BindingFlags.Instance)
							.SetValue(lavaVagao, 999f);
					}
				}

				if (lava)
				{
					var lavaTrem = trem.AddComponent<WWMO.MonoBehaviours.BoxTouchingLava_Mono>();
					lavaTrem.heatPercent = 1f;
					typeof(WWMO.MonoBehaviours.BoxTouchingLava_Mono)
						.GetField("heatDuration", BindingFlags.NonPublic | BindingFlags.Instance)
						.SetValue(lavaTrem, 9999f);
				}
			}

			foreach (var trem in trens)
			{
				Rigidbody2D rb = trem.GetComponent<Rigidbody2D>();
				if (rb == null) continue;

				Vector3 dir = cen.position - trem.transform.position;
				float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
				trem.transform.rotation = Quaternion.Euler(0, 0, angle);
				rb.velocity = trem.transform.right * speed;
			}
		}










		public static void SpawnBlock(Player player, TrainDirection direction)
		{
			LoadPrefab();
			if (blockPrefab == null) return;

			Vector3 spawnPos;
			Vector3 alertaPos;
			Quaternion spawnRotation = Quaternion.identity;

			if (direction == TrainDirection.LeftToRight)
			{
				Vector3 camLeftEdge = Camera.main.ViewportToWorldPoint(new Vector3(0, 0.5f, 0));
				spawnPos = new Vector3(camLeftEdge.x - 500f, player.transform.position.y, 0f);
				alertaPos = new Vector3(camLeftEdge.x + 3f, player.transform.position.y, 0f);
			}
			else
			{
				Vector3 camTopEdge = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0));
				spawnPos = new Vector3(player.transform.position.x, camTopEdge.y + 500f, 0f);
				alertaPos = new Vector3(player.transform.position.x, camTopEdge.y - 3f, 0f);
				spawnRotation = Quaternion.Euler(0f, 0f, 270f);
			}

			GameObject bloco = Object.Instantiate(blockPrefab, spawnPos, spawnRotation);

			var effects = player.GetComponent<BlockSpawnerEffects>();
			float scale = effects != null ? effects.blockScale : 2f;
			float speed = effects != null ? effects.blockSpeed : 200f;
			int vagoes = effects != null ? (int)effects.wagons : 0;
			bool lava = effects != null && effects.lava;
			bool bumerangue = effects != null && effects.boomerang;

			bloco.transform.localScale = new Vector3(scale, scale, 1f);
			ApplyGlowToAllSprites(bloco, Color.white, Color.white * 2f);

			Rigidbody2D rb = bloco.GetComponent<Rigidbody2D>();
			if (rb != null)
			{
				rb.gravityScale = 0f;
				rb.velocity = direction == TrainDirection.LeftToRight ?
					new Vector2(speed, 0f) :
					new Vector2(0f, -speed);

				rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
			}

			GameObject alertaGO = new GameObject("AlertaVisual");
			alertaGO.transform.position = alertaPos;
			alertaGO.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

			var srAlerta = alertaGO.AddComponent<SpriteRenderer>();
			srAlerta.sprite = SpriteLoader.LoadEmbeddedSprite("PRT.UI.Resources.alerta.png");
			srAlerta.sortingOrder = 100;

			var alertMat = new Material(Shader.Find("Sprites/Default"));
			alertMat.SetColor("_Color", Color.red);
			alertMat.EnableKeyword("_EMISSION");
			alertMat.SetColor("_EmissionColor", Color.red * 2f);
			srAlerta.material = alertMat;

			alertaGO.AddComponent<AlertBlink>();

			var autoDestroy = bloco.GetComponent<TrainAutoDestroy>();
			if (autoDestroy == null)
				autoDestroy = bloco.AddComponent<TrainAutoDestroy>();

			autoDestroy.bornFromAsterisco = false;

			autoDestroy.alertaVisual = alertaGO;
			autoDestroy.vagoes.Clear();
			autoDestroy.bumerangueAtivo = bumerangue;

			for (int i = 0; i < vagoes; i++)
			{
				GameObject vagao = Object.Instantiate(vagaoPrefab);
				vagao.transform.SetParent(bloco.transform, false);
				ApplyGlowToAllSprites(vagao, Color.white, Color.white * 2f);
				autoDestroy.vagoes.Add(vagao);

				if (lava)
				{
					var lavaVagao = vagao.AddComponent<WWMO.MonoBehaviours.BoxTouchingLava_Mono>();
					lavaVagao.heatPercent = 1f;
					typeof(WWMO.MonoBehaviours.BoxTouchingLava_Mono)
						.GetField("heatDuration", BindingFlags.NonPublic | BindingFlags.Instance)
						.SetValue(lavaVagao, 999f);
				}
			}

			if (lava)
			{
				var lavaTrem = bloco.AddComponent<WWMO.MonoBehaviours.BoxTouchingLava_Mono>();
				lavaTrem.heatPercent = 1f;
				typeof(WWMO.MonoBehaviours.BoxTouchingLava_Mono)
					.GetField("heatDuration", BindingFlags.NonPublic | BindingFlags.Instance)
					.SetValue(lavaTrem, 9999f);
			}

			if (bloco.GetComponent<TrainObject>() == null)
				bloco.AddComponent<TrainObject>();

			GameObject center = GameObject.Find("center");
			if (center == null)
			{
				center = new GameObject("center");
				Vector3 p = player.transform.position;
				p.z = 0f;
				center.transform.position = p;
			}

			bloco.transform.SetParent(center.transform, true);
		}

		private static void ApplyGlowToAllSprites(GameObject obj, Color baseColor, Color emissionColor)
		{
			var srs = obj.GetComponentsInChildren<SpriteRenderer>(true);
			foreach (var sr in srs)
			{
				var mat = new Material(Shader.Find("Sprites/Default"));
				mat.SetColor("_Color", baseColor);
				mat.EnableKeyword("_EMISSION");
				mat.SetColor("_EmissionColor", emissionColor);

				sr.material = mat;
				sr.sortingOrder = 100;
			}
		}
	}
}
