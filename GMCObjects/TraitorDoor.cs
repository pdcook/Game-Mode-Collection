using System.Collections;
using UnityEngine;
using GameModeCollection.GameModes;
using GameModeCollection.GameModes.TRT;
using UnboundLib;

namespace GameModeCollection.GMCObjects
{
    /// <summary>
    /// The main behaviour for the traitor door, which can be opened by traitors, and closes automatically after a brief period of time.
    /// Traitors can see the interact icon from all distances, as they stay on the edge of the screen similar to Radar points
    /// </summary>
    public class TraitorDoor : Interactable
    {
        /// <summary>
        /// Creates a new gameobject with the traitor interaction assigned to this door
        /// </summary>
        public override string HoverText { get; protected set; } = "Traitor Door";
        public override Color TextColor { get; protected set; } = GM_TRT.DullWhite;
        public override Color IconColor { get; protected set; } = GM_TRT.TraitorColor;
        public override bool InteractableInEditor { get; protected set; } = true;
        public override Alignment? RequiredAlignment { get; protected set; } = Alignment.Traitor;

        public const float MoveTime = 1f; // The time it takes to move the door
        public const float OpenTime = 1f; // The time the door remains open

        public Vector3 AutoOpenDetectorPosition { get; internal set; }
        public Vector3 ClosedPosition { get; private set; }
        public Vector3 OpenPosition { get; private set; }
        private Quaternion _savedRotation;
        public DoorState State { get; private set; } = DoorState.Closed;
        public bool AutoClose { get; private set; } = true;

        private float _timer = 0f; // The timer for the door

        new void Start()
        {
            base.Start();

            this.transform.GetChild(0).gameObject.GetOrAddComponent<TraitorDoorAutoOpener>().SetDoor(this);
            Rigidbody2D rig = this.transform.GetChild(0).gameObject.GetOrAddComponent<Rigidbody2D>();
            rig.isKinematic = true;

            // force child object to have constant size and rotation
            if (this.GetComponent<DetectMapEditor>()?.IsMapEditor ?? false)
            {
                this.transform.GetChild(0).gameObject.GetOrAddComponent<ForceScale>();
                this.transform.GetChild(0).gameObject.GetOrAddComponent<ForcePosition>();
            }

            this.CalculateClosePosition();
            this.CalculateOpenPosition();
        }
        private void SetForceTransforms(bool enabled)
        {
            this.transform.GetChild(0).gameObject.GetOrAddComponent<ForcePosition>().enabled = enabled;
        }        
        private void CalculateClosePosition()
        {
            // recalculate the close position when the door is closed (for the map editor)
            if (this.State == DoorState.Closed)
            {
                this.ClosedPosition = this.transform.position;
                return;
            }
        }
        private void CalculateOpenPosition()
        {
            this._savedRotation = this.transform.rotation; // save the rotation, so that if it changes later we can calculate the new open position (this is just for the map editor)
            // open the door by moving it a distance equal to its height in the local up direction
            this.OpenPosition = this.transform.position + this.transform.up * this.GetComponent<BoxCollider2D>().size.y * this.transform.lossyScale.y;
        }

        public override void OnInteract(Player player)
        {
            if (this.State == DoorState.Closed || this.State == DoorState.Closing)
            {
                this.Open();
            }
            else if (this.State == DoorState.Open || this.State == DoorState.Opening)
            {
                this.Close();
            }
        }
        public void Open(bool autoClose = true)
        {
            this.State = DoorState.Opening;
            this._timer = MoveTime;
            this.AutoClose = autoClose;
        }
        public void Close()
        {
            this.State = DoorState.Closing;
            this._timer = MoveTime;
        }
        void Update()
        {
            if (this.transform.rotation != this._savedRotation)
            {
                this.CalculateOpenPosition();
            }
            if (this._timer > 0f)
            {
                this.SetForceTransforms(true);
                this._timer -= TimeHandler.deltaTime;
                switch (this.State)
                {
                    case DoorState.Opening:
                        this.transform.position = Vector3.MoveTowards(this.transform.position, this.OpenPosition, TimeHandler.deltaTime * (this.OpenPosition - this.ClosedPosition).magnitude / MoveTime);
                        break;
                    case DoorState.Closing:
                        this.transform.position = Vector3.MoveTowards(this.transform.position, this.ClosedPosition, TimeHandler.deltaTime * (this.ClosedPosition - this.OpenPosition).magnitude / MoveTime);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (this.State)
                {
                    case DoorState.Opening:
                        this.State = DoorState.Open;
                        this.SetForceTransforms(false);
                        if (this.AutoClose)
                        {
                            this._timer = OpenTime;
                        }
                        break;
                    case DoorState.Open:
                        if (this.AutoClose)
                        {
                            this.State = DoorState.Closing;
                            this._timer = OpenTime;
                        }
                        break;
                    case DoorState.Closing:
                        this.State = DoorState.Closed;
                        this.SetForceTransforms(false);
                        break;
                    case DoorState.Closed:
                        this.CalculateClosePosition();
                        this.CalculateOpenPosition();
                        break;
                    default:
                        break;
                }
            }
        }            
        public enum DoorState { Opening, Open, Closing, Closed }
    }
    
    class TraitorDoorAutoOpener : MonoBehaviour
    {
        CircleCollider2D trigger = null;
        TraitorDoor Door = null;
        void Start()
        {
            this.gameObject.layer = LayerMask.NameToLayer("PlayerObjectCollider");
            this.trigger = this.gameObject.GetOrAddComponent<CircleCollider2D>();
            this.trigger.isTrigger = true;
            this.trigger.radius = 2f;
        }
        internal void SetDoor(TraitorDoor door)
        {
            this.Door = door;
        }
        void OnTriggerEnter2D(Collider2D collider2D)
        {
            if (this.Door is null) { return; }

            // if the collider is a player and has LoS to this object, then open the door
            if (this.Door.State == TraitorDoor.DoorState.Closed
                && collider2D?.GetComponent<Player>() != null
                && !collider2D.GetComponent<Player>().data.dead
                && PlayerManager.instance.CanSeePlayer(this.transform.position, collider2D.GetComponent<Player>()).canSee)
            {
                this.Door.Open(true);
            }
        }
    }
}
