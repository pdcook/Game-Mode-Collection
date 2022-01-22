using System.Collections;
using UnityEngine;
using UnboundLib;
using UnboundLib.GameModes;

public class CrownHandler : MonoBehaviour
{
	private float crownPos;
	public AnimationCurve transitionCurve;
	private int currentCrownHolder = -1;
	private int previousCrownHolder = -1;
	internal Rigidbody2D Rig => this.GetComponent<Rigidbody2D>();
	internal CircleCollider2D Col => this.GetComponent<CircleCollider2D>();
	public int CrownHolder => this.currentCrownHolder;
	internal static CrownHandler MakeCrownHandler(Transform parent)
	{
		GM_ArmsRace gm = GameModeManager.GetGameMode<GM_ArmsRace>(GameModeManager.ArmsRaceID);

		GameObject crown = GameObject.Instantiate(gm.gameObject.transform.GetChild(0).gameObject, parent);
		crown.name = "Crown";
		CrownHandler crownHandler = crown.AddComponent<CrownHandler>();
		crownHandler.transitionCurve = new AnimationCurve((Keyframe[])crown.GetComponent<GameCrownHandler>().transitionCurve.InvokeMethod("GetKeys"));
		crownHandler.gameObject.AddComponent<Rigidbody2D>();
		crownHandler.gameObject.AddComponent<CircleCollider2D>();

		UnityEngine.GameObject.DestroyImmediate(crown.GetComponent<GameCrownHandler>());

		return crownHandler;
	}

	void Start()
    {
		this.transform.localScale = Vector3.one;
		this.transform.GetChild(0).localScale = new Vector3(0.5f, 0.4f, 1f);
    }

	public void SetCrownPos(Vector3 position)
    {
		this.transform.position = position;
	}

	void Update()
    {
		if (this.currentCrownHolder != -1)
		{
			this.Rig.isKinematic = true;
			this.Col.enabled = false;
		}
		else
        {
			this.Rig.isKinematic = false;
			this.Col.enabled = true;
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
		this.previousCrownHolder = this.currentCrownHolder == -1 ? playerID : this.currentCrownHolder;
		this.currentCrownHolder = playerID;
		if (this.currentCrownHolder != -1) { base.StartCoroutine(this.IGiveCrownToPlayer()); }
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
