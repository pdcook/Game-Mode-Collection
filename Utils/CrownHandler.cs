using System.Collections;
using UnityEngine;
using UnboundLib;
using UnboundLib.GameModes;
using MapEmbiggener;
using System.Linq;

public class CrownHandler : MonoBehaviour
{
	private const float MaxFreeTime = 10f;

	private bool hidden = true;
	private float crownPos;
	public AnimationCurve transitionCurve;
	private int currentCrownHolder = -1;
	private int previousCrownHolder = -1;
	internal Rigidbody2D Rig => this.GetComponent<Rigidbody2D>();
	internal BoxCollider2D Col => this.GetComponent<BoxCollider2D>();
	public int CrownHolder => this.currentCrownHolder;

	private float freeFor = 0f;
	private float _heldFor = 0f;
	public float HeldFor
	{
		get
        {
			return this._heldFor;
        }
		private set
        {
			this._heldFor = value;
        }
	} 

	internal static CrownHandler MakeCrownHandler(Transform parent)
	{
		GM_ArmsRace gm = GameModeManager.GetGameMode<GM_ArmsRace>(GameModeManager.ArmsRaceID);

		GameObject crown = GameObject.Instantiate(gm.gameObject.transform.GetChild(0).gameObject, parent);
		crown.name = "Crown";
		CrownHandler crownHandler = crown.AddComponent<CrownHandler>();
		crownHandler.transitionCurve = new AnimationCurve((Keyframe[])crown.GetComponent<GameCrownHandler>().transitionCurve.InvokeMethod("GetKeys"));
		crownHandler.gameObject.AddComponent<Rigidbody2D>();
		BoxCollider2D bCol = crownHandler.gameObject.AddComponent<BoxCollider2D>();
		bCol.size = new Vector2(1f, 0.5f);
		bCol.edgeRadius = 0.1f;

		UnityEngine.GameObject.DestroyImmediate(crown.GetComponent<GameCrownHandler>());

		return crownHandler;
	}
	void Start()
    {
		this.transform.localScale = Vector3.one;
		this.transform.GetChild(0).localScale = new Vector3(0.5f, 0.4f, 1f);
    }

	public void Reset()
    {
		this.hidden = true;
		this.currentCrownHolder = -1;
		this.previousCrownHolder = -1;
    }

	/// <summary>
	/// takes in a NORMALIZED vector between (0,0,0) and (1,1,0) which represents the percentage across the bounds on each axis
	/// </summary>
	/// <param name="normalized_position"></param>
	public void Spawn(Vector3 normalized_position)
    {
		this.hidden = false;
		this.SetPos(OutOfBoundsUtils.GetPoint(normalized_position));
		this.SetVel(Vector2.zero);
		this.SetRot(0f);
		this.SetAngularVel(0f);
    }

	public void SetPos(Vector3 position)
    {
		this.GiveCrownToPlayer(-1);
		this.transform.position = position;
	}
	public void SetVel(Vector2 velocity)
    {
		this.Rig.velocity = velocity;
    }

	public void SetAngularVel(float angularVelocity)
    {
		this.Rig.angularVelocity = angularVelocity;
    }

	public void SetRot(float rot)
    {
		this.Rig.rotation = rot;
    }
    private Vector3 GetFarthestSpawnFromPlayers()
    {
        Vector3[] spawns = MapManager.instance.GetSpawnPoints().Select(s => s.localStartPos).ToArray();
        float dist = -1f;
        Vector3 best = Vector3.zero;
        foreach (Vector3 spawn in spawns)
        {
            float thisDist = PlayerManager.instance.players.Where(p => !p.data.dead).Select(p => Vector3.Distance(p.transform.position, spawn)).Sum();
            if (thisDist > dist)
            {
                dist = thisDist;
                best = spawn;
            }
        }
        return best;
    }

	void OnCollisionEnter2D(Collision2D collision2D)
    {
		int? playerID = collision2D?.collider?.GetComponent<Player>()?.playerID;
		if (playerID != null)
        {
			this.GiveCrownToPlayer((int)playerID);
        }
    }

	void Update()
    {
		if (this.currentCrownHolder != -1 || this.hidden)
		{
			this.HeldFor += TimeHandler.deltaTime;

			this.Rig.isKinematic = true;
			this.SetRot(0f);
			this.SetAngularVel(0f);
			this.Col.enabled = false;
			if (this.hidden) { this.SetPos(100000f * Vector2.up); }
		}
		else
        {
			this.freeFor += TimeHandler.deltaTime;

			this.Rig.isKinematic = false;
			this.Col.enabled = true;
			if (!OutOfBoundsUtils.IsInsideBounds(this.transform.position, out Vector3 _) || this.freeFor > CrownHandler.MaxFreeTime)
            {
				OutOfBoundsUtils.IsInsideBounds(GetFarthestSpawnFromPlayers(), out Vector3 newSpawn);
				this.Spawn(newSpawn);
            }
        }
    }

	void LateUpdate()
	{
		if (this.currentCrownHolder == -1 || this.previousCrownHolder == -1)
		{
			return;
		}
		Vector3 position = Vector3.LerpUnclamped((Vector3)PlayerManager.instance.players[this.previousCrownHolder].data.InvokeMethod("GetCrownPos"), (Vector3)PlayerManager.instance.players[this.currentCrownHolder].data.InvokeMethod("GetCrownPos"), this.crownPos);
		base.transform.position = position;
	}


	public void GiveCrownToPlayer(int playerID)
	{
		this.HeldFor = 0f;
		this.freeFor = 0f;
		this.previousCrownHolder = this.currentCrownHolder == -1 ? playerID : this.currentCrownHolder;
		this.currentCrownHolder = playerID;
		if (this.currentCrownHolder != -1 && !this.hidden) { base.StartCoroutine(this.IGiveCrownToPlayer()); }
	}


	private IEnumerator IGiveCrownToPlayer()
	{
		for (float i = 0f; i < this.transitionCurve.keys[this.transitionCurve.keys.Length - 1].time; i += Time.unscaledDeltaTime)
		{
			this.crownPos = Mathf.LerpUnclamped(0f, 1f, this.transitionCurve.Evaluate(i));
			yield return null;
		}
		yield break;
	}
}
