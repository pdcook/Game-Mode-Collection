using GameModeCollection.GameModes;
using MapEmbiggener;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnboundLib.GameModes;
using UnityEngine;
using TMPro;
using System;
using UnboundLib.Networking;

namespace GameModeCollection.Objects.GameModeObjects.TRT
{
	public static class C4Prefab
	{
		private readonly static PlayerSkin DefaultC4SkinColors = new PlayerSkin()
		{
			winText = Color.white,
			color = new Color32(100, 100, 100, 255),
			backgroundColor = Color.black,
			particleEffect = Color.gray
        };

		private static GameObject _C4 = null;

		public static GameObject C4
		{
			get
			{
				if (C4Prefab._C4 == null)
				{

					GameObject c4 = new GameObject("C4Prefab", typeof(PhotonView), typeof(C4Handler));
					ObjectParticleSkin.AddObjectParticleSkin(c4.transform, Sprites.Box, DefaultC4SkinColors);
					GameObject Clock = new GameObject("C4 Clock",typeof(TextMeshPro));
					Clock.GetComponent<TextMeshPro>().color = Color.red;
					Clock.transform.SetParent(c4.transform);
					Clock.transform.localPosition = Vector3.zero;
					Clock.transform.localScale = Vector3.one;
					Clock.GetComponent<TextMeshPro>().enabled = false;
					Clock.GetComponent<TextMeshPro>().alignment = TextAlignmentOptions.Center;
					Clock.GetComponent<TextMeshPro>().fontSize = 10;

					c4.GetComponent<C4Handler>().IsPrefab = true;

					GameModeCollection.Log("C4 Prefab Instantiated");
					UnityEngine.GameObject.DontDestroyOnLoad(c4);

					PhotonNetwork.PrefabPool.RegisterPrefab(c4.name, c4);

					C4Prefab._C4 = c4;
				}
				return C4Prefab._C4;
			}
		}


	}
	public class C4Handler : NetworkPhysicsItem<BoxCollider2D, CircleCollider2D>
	{
		private const float TriggerRadius = 1.5f;

		public bool IsPrefab { get; internal set; } = false;

		internal SpriteRenderer Renderer => this.gameObject.GetComponentInChildren<SpriteRenderer>();
		public float Time { get; private set; } = float.MaxValue;
		public int PlacerID { get; private set; } = -1;

		private TextMeshPro Clock => this.GetComponentInChildren<TextMeshPro>();

		public override void OnPhotonInstantiate(PhotonMessageInfo info)
		{
			object[] data = info.photonView.InstantiationData;

			this.PlacerID = (int)data[0];
			this.Time = (float)data[1];
		}
		internal static IEnumerator MakeC4Handler(int placerID, float time, Vector3 position, Quaternion rotation)
		{
			if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
			{
				PhotonNetwork.Instantiate(
                        C4Prefab.C4.name,
                        position,
                        rotation,
                        0,
                        new object[] { placerID, time }
					);
			}

			yield break;
		}
		internal static IEnumerator AskHostToMakeC4(int placerID, float time, Vector3 position, Quaternion rotation)
        {
			NetworkingManager.RPC(typeof(C4Handler), nameof(RPCM_MakeC4Handler), placerID, time, position, rotation);
			yield break;
        }
		[UnboundRPC]
		private static void RPCM_MakeC4Handler(int placerID, float time, Vector3 position, Quaternion rotation)
        {
			GameModeCollection.instance.StartCoroutine(MakeC4Handler(placerID, time, position, rotation));	
        }
		protected override void Awake()
		{
			this.PhysicalProperties = new ItemPhysicalProperties(mass: 80000f, bounciness: 0f,
																	playerPushMult: 0f,
																	playerDamageMult: 0f,
																	collisionDamageThreshold: float.MaxValue,
																	friction: 1f,
																	impulseMult: 0f,
																	forceMult: 0f, visibleThroughShader: false);

			base.Awake();
		}
		protected override void Start()
		{
			this.transform.localScale = new Vector3(1f, 0.5f, 1f);
			this.Clock.transform.localScale = new Vector3(0.5f, 1f, 1f);
			this.GetComponentInChildren<PlayerSkinParticle>().transform.localPosition = Vector3.zero;

			base.Start();

			this.Trig.radius = C4Handler.TriggerRadius;
			this.Col.size = new Vector2(0.7f, 0.7f);
			this.Col.edgeRadius = 0.1f;

			if (this.IsPrefab)
            {
				this.SetPos(1000000f * Vector2.one);
				this.Rig.isKinematic = true;
				this.gameObject.SetActive(false);
            }
			else
            {
				this.Rig.isKinematic = false;
				this.gameObject.SetActive(true);
            }
			this.Renderer.color = new Color32 (100, 100, 100, 255);
			this.Renderer.enabled = false;
		}

		protected internal override void OnTriggerEnter2D(Collider2D collider2D)
		{
			Player player = collider2D?.GetComponent<Player>();
			if (player != null && player.data.view.IsMine && this.CanSeePlayer(player))
			{
				// display time
				Clock.enabled = true;
			}
			else if (player != null)
            {
				Clock.enabled = false;
            }
			base.OnTriggerEnter2D(collider2D);
		}
 		protected internal override void OnTriggerStay2D(Collider2D collider2D)
		{
			Player player = collider2D?.GetComponent<Player>();
			if (player != null && player.data.view.IsMine && this.CanSeePlayer(player))
			{
				// display time
				Clock.enabled = true;
			}
			else if (player != null)
            {
				Clock.enabled = false;
            }
			base.OnTriggerStay2D(collider2D);
		}
       protected internal override void OnTriggerExit2D(Collider2D collider2D)
        {
			Player player = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == collider2D?.GetComponent<Player>()?.playerID);
			if (player != null && player.data.view.IsMine)
			{
				Clock.enabled = false;
			}
            base.OnTriggerExit2D(collider2D);
        }
        private bool CanSeePlayer(Player player)
		{
			RaycastHit2D[] array = Physics2D.RaycastAll(this.transform.position, (player.data.playerVel.position - (Vector2)this.transform.position).normalized, Vector2.Distance(this.transform.position, player.data.playerVel.position), PlayerManager.instance.canSeePlayerMask);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].transform
					&& !array[i].transform.root.GetComponent<SpawnedAttack>()
					&& !array[i].transform.root.GetComponent<Player>()
					&& !array[i].transform.root.GetComponent<C4Handler>()
					)
				{
					return false;
				}
			}
			return true;
		}
		string GetClockString(float time_in_seconds)
		{
			if (time_in_seconds < 10f)
            {
				return "XX:XX";
            }
			if (time_in_seconds > 6000f)
            {
				return "";
            }
			return TimeSpan.FromSeconds(time_in_seconds).ToString(@"mm\:ss");
		}
		protected override void Update()
        {
			this.Time -= TimeHandler.deltaTime;

            base.Update();

			Clock.text = GetClockString(this.Time);
			SetClockPositionAndRotation();
        }
		void SetClockPositionAndRotation()
        {
			this.Clock.transform.position = this.transform.position + 2f * Vector3.up;
			this.Clock.transform.rotation = Quaternion.identity;
        }

        private const string SyncedTimeKey = "C4_Time";

		protected override void SetDataToSync()
		{
			this.SetSyncedFloat(SyncedTimeKey, this.Time);
		}
		protected override void ReadSyncedData()
		{
			// syncing
			this.Time = this.GetSyncedFloat(SyncedTimeKey, this.Time);
		}
        protected override bool SyncDataNow()
        {
			return true;
        }
    }
}
