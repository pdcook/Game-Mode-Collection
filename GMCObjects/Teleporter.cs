using UnityEngine;
using GameModeCollection.GameModes;
using UnboundLib;
namespace GameModeCollection.GMCObjects
{
    public class Teleporter : MonoBehaviour
    {
        void Start()
        {
            this.transform.GetChild(0).gameObject.GetOrAddComponent<TeleporterBase>();
            this.transform.GetChild(1).gameObject.GetOrAddComponent<TeleporterBase>();
        }
    }
    public class TeleporterBase : Interactable
    {
        public const float RechargeTime = 3f;
        public static readonly Color RechargingColor = new Color32(230, 230, 230, 128);
        public override string HoverText { get; protected set; } = "Teleporter";
        public override Color TextColor { get; protected set; } = GM_TRT.DullWhite;
        public override Color IconColor { get; protected set; } = GM_TRT.DullWhite;
        public override float VisibleDistance { get; protected set; } = 5f;
        private TeleporterBase PairedTeleporter = null;
        float timer = 0f;

        new void Start()
        {
            base.Start();

            foreach (TeleporterBase teleporter in this.transform.parent.GetComponentsInChildren<TeleporterBase>())
            {
                if (teleporter != this)
                {
                    this.PairedTeleporter = teleporter;
                    break;
                }
            }

        }
        public void StartRecharge()
        {
            this.InteractionUI.SetText("<size=50%>Recharging...");
            this.InteractionUI.SetTextColor(RechargingColor);
            this.timer = RechargeTime;
        }
        public override void OnInteract(Player player)
        {
            if (this.PairedTeleporter is null || player is null || this.timer > 0f) { return; }
            this.StartRecharge();
            this.PairedTeleporter.StartRecharge();
            this.PlayParts(player);

            // disable the player's wall collision
            player.GetComponentInParent<PlayerCollision>().IgnoreWallForFrames(2);

            // teleport the player's wobble objects
            player.GetComponentInChildren<PlayerWobblePosition>().transform.position = this.PairedTeleporter.transform.position;
            player.GetComponentInChildren<PlayerWobblePosition>().SetFieldValue("physicsPos", this.PairedTeleporter.transform.position);
            player.GetComponentInChildren<PlayerWobblePosition>().SetFieldValue("velocity", Vector3.zero);

            // teleport the player
            player.transform.position = this.PairedTeleporter.transform.position;
            
            // Moves the hand? container to the player's position
            FollowTransform follow = ((HoldingObject)player.data.GetComponent<Aim>().GetFieldValue("holdingObject")).GetComponent<FollowTransform>();
            Vector3 startPos = follow.transform.position;
            follow.InvokeMethod("LateUpdate");
            Vector3 travel = follow.transform.position - startPos;

            // Moves the gun the same distance to avoid Lerp
            player.GetComponent<Holding>().holdable.rig.transform.position += travel;
            
            this.PlayParts(player);
        }
        public void PlayParts(Player player)
        {
            PlayerJump playerJump = player.GetComponent<PlayerJump>();
            if (playerJump != null)
            {
                for (int i = 0; i < playerJump.jumpPart.Length; i++)
                {
                    playerJump.jumpPart[i].transform.position = player.transform.position;
                    playerJump.jumpPart[i].transform.rotation = Quaternion.LookRotation(new Vector3(0f, 1f, 0f));
                    playerJump.jumpPart[i].Play();
                }
            }
        }
        void Update()
        {
            if (this.timer > 0f)
            {
                this.timer -= TimeHandler.deltaTime;
                if (this.timer <= 0f)
                {
                    this.InteractionUI.SetText(this.HoverText);
                    this.InteractionUI.SetTextColor(this.TextColor);
                }
            }
        }
    }
}
