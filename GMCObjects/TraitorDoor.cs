using System.Collections;
using UnityEngine;
using GameModeCollection.GameModes;
using GameModeCollection.GameModes.TRT;

namespace GameModeCollection.GMCObjects
{
    /// <summary>
    /// The main behaviour for the traitor door, which can be opened by traitors, and closes automatically after a brief period of time.
    /// Traitors can see the interact icon from all distances, as they stay on the edge of the screen similar to Radar points
    /// </summary>
    public class TraitorDoor : TraitorInteractable
    {
        /// <summary>
        /// Creates a new gameobject with the traitor interaction assigned to this door
        /// </summary>
        public override string HoverText { get; protected set; } = "Traitor Door";
        public override Color TextColor { get; protected set; } = GM_TRT.DullWhite;

        public const float MoveTime = 1f; // The time it takes to move the door
        public const float OpenTime = 1f; // The time the door remains open

        public Vector3 ClosedPosition { get; private set; }
        public Vector3 OpenPosition { get; private set; }
        private Quaternion _savedRotation;
        public DoorState State { get; private set; } = DoorState.Closed;
        public bool AutoClose { get; private set; } = true;

        private float _timer = 0f; // The timer for the door

        new void Start()
        {
            base.Start();

            this.CalculateClosePosition();
            this.CalculateOpenPosition();
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
            this.OpenPosition = this.transform.position + this.transform.up * this.GetComponent<BoxCollider2D>().size.y * this.transform.localScale.y;
        }

        public override void OnInteract(Player player)
        {
            GameModeCollection.Log($"{HoverText} INTERACT");
            if (GameModeCollection.DEBUG || (player != null && !player.data.dead && RoleManager.GetPlayerAlignment(player) == Alignment.Traitor))
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
}
