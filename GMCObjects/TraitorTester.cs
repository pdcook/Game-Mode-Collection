using UnityEngine;
using GameModeCollection.GameModes;
using UnboundLib;
using UnboundLib.Utils;
using System.Linq;
using GameModeCollection.GameModes.TRT;
using System.Collections.Generic;
using System.Collections;
namespace GameModeCollection.GMCObjects
{
    public class ForceTransform : MonoBehaviour
    {
        Vector3 parentPosLastFrame;
        Vector3 parentScaleLastFrame;
        Vector3 positionLastFrame;
        Quaternion parentRotationLastFrame;
        void Start()
        {
            if (this.transform.parent is null)
            {
                Destroy(this);
                return;
            }
            this.parentPosLastFrame = this.transform.parent.position;
            this.parentScaleLastFrame = this.transform.parent.localScale;
            this.parentRotationLastFrame = this.transform.parent.rotation;
            this.positionLastFrame = this.transform.position;
            
            this.transform.rotation = Quaternion.identity;
            Transform parent = this.transform.parent;
            this.transform.SetParent(null);
            this.transform.localScale = Vector3.one;
            this.transform.SetParent(parent);
        }
        void LateUpdate()
        {
            Transform parent = this.transform.parent;

            if (this.parentPosLastFrame != parent.position
                || this.parentScaleLastFrame != parent.localScale
                || this.parentRotationLastFrame != parent.rotation
                )
            {
                this.transform.rotation = Quaternion.identity;
                this.transform.position = this.positionLastFrame;
                this.transform.SetParent(null);
                this.transform.localScale = Vector3.one;
                this.transform.SetParent(parent);
            }

        }
        void FixedUpdate()
        {
            this.positionLastFrame = this.transform.position;
            this.parentPosLastFrame = this.transform.parent.position;
            this.parentScaleLastFrame = this.transform.parent.localScale;
            this.parentRotationLastFrame = this.transform.parent.rotation;
        }
    }

    public class TraitorTester : MonoBehaviour
    {
        void Start()
        {
            // button offset vertically from the actual object
            GameObject button = new GameObject("Button", typeof(TraitorTesterBase), typeof(DetectMapEditor));
            button.transform.SetParent(this.transform.GetChild(0));
            button.transform.localPosition = new Vector3(0f, -2f, 0f);
            button.GetComponent<DetectMapEditor>().IsMapEditor = this.transform.GetChild(0)?.GetComponent<DetectMapEditor>()?.IsMapEditor ?? false;
            TraitorTesterBase tt = button.GetOrAddComponent<TraitorTesterBase>();
            // room
            tt.SetRoom(this.transform.GetChild(1).gameObject);
            // light
            tt.SetLight(this.transform.GetChild(2).GetChild(0).gameObject);
            // door
            tt.SetDoor(this.transform.gameObject);

            // force all child objects to have constant sizes and rotation
            if (button.GetComponent<DetectMapEditor>().IsMapEditor)
            {
                this.transform.GetChild(0).gameObject.GetOrAddComponent<ForceTransform>();
                this.transform.GetChild(1).gameObject.GetOrAddComponent<ForceTransform>();
                this.transform.GetChild(2).gameObject.GetOrAddComponent<ForceTransform>();
            }
            /*
            else
            {
                this.transform.GetChild(2).SetParent(this.transform.parent);
                this.transform.GetChild(1).SetParent(this.transform.parent);
                this.transform.GetChild(0).SetParent(this.transform.parent);
            }*/
        }
    }
    public class TraitorTesterBase : Interactable
    {
        public static int NumberOfUses
        {
            get
            {
                int numPlayers = PlayerManager.instance.players.Count();
                if (numPlayers > 6)
                {
                    return 2;
                }
                else
                {
                    return 1;
                }
            }
        }

        // colors for the light
        public static readonly Color TraitorColor = GM_TRT.TraitorColor;
        public static readonly Color InnocentColor = GM_TRT.InnocentColor;
        public static readonly Color ErrorColor = new Color32(252, 152, 3, 255);
        public static readonly Color IdleColor = new Color32(100, 100, 100, 255);
        
        public const float RechargeTime = 90f; // reusable after 90 seconds
        public const float RecalibrateTime = 15f; // on error, become reusable after 15 seconds
        
        public const float DisplayColorFor = 10f;

        public const float CloseDoorFor = 5f;
        public const float DisplayResultsAfter = 3f;

        public static readonly Color RechargingColor = new Color32(230, 230, 230, 128);
        public override string HoverText { get; protected set; } = "Traitor Tester";
        public override Color TextColor { get; protected set; } = GM_TRT.DullWhite;
        public override Color IconColor { get; protected set; } = GM_TRT.DullWhite;
        public override float VisibleDistance { get; protected set; } = 5f;
        public override bool InteractableInEditor { get; protected set; } = true;
        public override bool RequireLoS { get; protected set; } = true;
        private GameObject RoomObj = null;
        private GameObject LightObj = null;
        private GameObject DoorObj = null;
        private TesterDoor Door => DoorObj?.GetComponent<TesterDoor>();
        private UnityEngine.UI.Image Light => LightObj?.GetComponent<UnityEngine.UI.Image>();
        private int uses = 0;
        private TimeSince resetTimer;
        private TimeSince colorTimer;
        private bool recalibrating = false;
        private bool recharging = false;
        private bool isTesting = false;
        private bool isShowingResult = false;

        internal void SetRoom(GameObject room)
        {
            RoomObj = room;
        }
        internal void SetLight(GameObject light)
        {
            LightObj = light;
        }
        internal void SetDoor(GameObject door)
        {
            DoorObj = door;
        }
        private void SetLightColor(Color color, bool idle = false)
        {
            if (this.Light is null) { return; }
            this.Light.color = color;

            this.isShowingResult = !idle;
            if (!idle)
            {
                this.colorTimer = 0f;
            }
        }

        new void Start()
        {
            this.SetLightColor(IdleColor, true);
            base.Start();
        }
        public void StartRecharge()
        {
            this.recharging = true;
            this.InteractionUI.SetText("<size=50%>Recharging...");
            this.InteractionUI.SetTextColor(RechargingColor);
            this.resetTimer = 0f;
        }
        public void StartRecalibrate()
        {
            this.recalibrating = true;
            this.InteractionUI.SetText("<size=50%>Recalibrating...");
            this.InteractionUI.SetTextColor(RechargingColor);
            this.resetTimer = 0f;
        }
        public override void OnInteract(Player player)
        {
            if (!(this.IsEditorObj && this.InteractableInEditor)
                && (this.RoomObj is null
                || this.LightObj is null
                || this.DoorObj is null
                || player is null
                || this.isTesting
                || this.recalibrating
                || this.recharging
                || this.uses > NumberOfUses)) { return; }

            this.StartCoroutine(IDoTest());

        }
        
        IEnumerator IDoTest()
        {
            this.isTesting = true;

            // close the door
            this.Door.Close();

            // wait for the door to be closed
            yield return new WaitUntil(() => this.Door.State == TesterDoor.DoorState.Closed);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // do the test
            Alignment? result = this.RunTester();

            // wait to show the results
            yield return new WaitForSeconds(DisplayResultsAfter);

            switch (result)
            {
                case Alignment.Innocent:
                    this.SetLightColor(InnocentColor);
                    if (!this.IsEditorObj)
                    {
                        this.uses++;
                    }
                    break;
                case Alignment.Traitor:
                    this.SetLightColor(TraitorColor);
                    if (!this.IsEditorObj)
                    {
                        this.uses++;
                    }
                    break;
                case null:
                default:
                    this.SetLightColor(ErrorColor);
                    break;
            }

            // wait to open the door
            yield return new WaitForSeconds(CloseDoorFor-DisplayResultsAfter);

            // open the door
            this.Door.Open();

            // wait until the door is open
            yield return new WaitUntil(() => this.Door.State == TesterDoor.DoorState.Open);

            // start recharging/recalibrating
            if (result is null)
            {
                this.StartRecalibrate();
            }
            else
            {
                this.StartRecharge();
            }

            this.isTesting = false;

            yield break; 
        }

        Alignment? RunTester()
        {
            // return either Traitor or Innocent, or null if the wrong number of players are in the room
            Player[] playersInRoom = PlayerManager.instance.players.Where(p =>
                    PlayerManager.instance.CanSeePlayer(this.RoomObj.transform.position, p).canSee).ToArray();

            if (playersInRoom.Count() != 2)
            {
                return null;
            }

            /// TESTER RULES
            /// 
            /// Chaos and Killer count as Innocent
            /// If any player is missing an alignment, return null (error)
            /// If both players are the same alignment (NO MATTER WHAT THEY ARE) then return Innocent
            /// If the players are different alignments, then return Traitor

            Alignment? align1 = RoleManager.GetPlayerAlignment(playersInRoom[0]);
            Alignment? align2 = RoleManager.GetPlayerAlignment(playersInRoom[1]);

            if (align1 is null || align2 is null)
            {
                return null;
            }

            Alignment testAlign1 = align1.Value == Alignment.Traitor ? Alignment.Traitor : Alignment.Innocent;
            Alignment testAlign2 = align2.Value == Alignment.Traitor ? Alignment.Traitor : Alignment.Innocent;

            if (testAlign1 == testAlign2)
            {
                return Alignment.Innocent;
            }
            else
            {
                return Alignment.Traitor;
            }
        }

        void Update()
        {

            if (this.isShowingResult && this.colorTimer > DisplayColorFor)
            {
                this.SetLightColor(IdleColor, true);
            }
            
            if (this.recharging)
            {
                this.InteractionUI.SetText($"<size=50%>Recharging...\n{RechargeTime - this.resetTimer.Relative:N0}");
                this.InteractionUI.SetTextColor(RechargingColor);
                if (this.resetTimer > RechargeTime)
                {
                    this.InteractionUI.SetText(this.HoverText);
                    this.InteractionUI.SetTextColor(this.TextColor);
                    this.recharging = false;
                }
            }
            else if (this.recalibrating)
            {
                this.InteractionUI.SetText($"<size=50%>Recalibrating...\n{RecalibrateTime - this.resetTimer.Relative:N0}");
                this.InteractionUI.SetTextColor(RechargingColor);
                if (this.resetTimer > RecalibrateTime)
                {
                    this.InteractionUI.SetText(this.HoverText);
                    this.InteractionUI.SetTextColor(this.TextColor);
                    this.recalibrating = false;
                }
            }
            else
            {
                this.InteractionUI.SetText(this.HoverText);
                this.InteractionUI.SetTextColor(this.TextColor);
            }
        }
    }
    public class TesterDoor : MonoBehaviour
    {
        public const float MoveTime = 1f; // The time it takes to move the door

        public Vector3 OpenPosition { get; private set; }
        public Vector3 ClosePosition { get; private set; }
        private Quaternion _savedRotation;
        public DoorState State { get; private set; } = DoorState.Open;

        private float _timer = 0f; // The timer for the door

        void Start()
        {
            this.CalculateOpenPosition();
            this.CalculateClosePosition();
        }
        private void CalculateOpenPosition()
        {
            // recalculate the close position when the door is closed (for the map editor)
            if (this.State == DoorState.Open)
            {
                this.OpenPosition = this.transform.position;
                return;
            }
        }
        private void CalculateClosePosition()
        {
            this._savedRotation = this.transform.rotation; // save the rotation, so that if it changes later we can calculate the new open position (this is just for the map editor)
            // open the door by moving it a distance equal to its height in the local down direction
            this.ClosePosition = this.transform.position - this.transform.up * this.GetComponent<BoxCollider2D>().size.y * this.transform.localScale.y;
        }

        public void Close()
        {
            this.State = DoorState.Closing;
            this._timer = MoveTime;
        }
        public void Open()
        {
            this.State = DoorState.Opening;
            this._timer = MoveTime;
        }
        void Update()
        {
            if (this.transform.rotation != this._savedRotation)
            {
                this.CalculateClosePosition();
            }
            if (this._timer > 0f)
            {
                this._timer -= TimeHandler.deltaTime;
                switch (this.State)
                {
                    case DoorState.Closing:
                        this.transform.position = Vector3.MoveTowards(this.transform.position, this.ClosePosition, TimeHandler.deltaTime * (this.ClosePosition - this.OpenPosition).magnitude / MoveTime);
                        break;
                    case DoorState.Opening:
                        this.transform.position = Vector3.MoveTowards(this.transform.position, this.OpenPosition, TimeHandler.deltaTime * (this.OpenPosition - this.ClosePosition).magnitude / MoveTime);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (this.State)
                {
                    case DoorState.Closing:
                        this.State = DoorState.Closed;
                        break;
                    case DoorState.Closed:
                        break;
                    case DoorState.Opening:
                        this.State = DoorState.Open;
                        break;
                    case DoorState.Open:
                        this.CalculateOpenPosition();
                        this.CalculateClosePosition();
                        break;
                    default:
                        break;
                }
            }
        }
        public enum DoorState { Closing, Closed, Opening, Open }
    }
}
