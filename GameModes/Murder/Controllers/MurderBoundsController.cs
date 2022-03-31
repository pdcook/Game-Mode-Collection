using MapEmbiggener.Controllers;
using GameModeCollection.GameModes.Murder;
using UnityEngine;
using MapEmbiggener;

namespace GameModeCollection.GameModes.Murder.Controllers
{
    public class MurderBoundsController : BoundsController
    {
        public static string ControllerID => "Murder_BoundsController";
        public override void SetDataToSync()
        {
        }

        public override void ReadSyncedData()
        {
        }

        public override bool SyncDataNow()
        {
            return true;
        }
        public override void OnUpdate()
        {
            Vector2 mult = MurderMapManager.GetMapBounds(MurderMapManager.GetMapFromMapID(MurderMapManager.CurrentLevel));
            this.MaxXTarget = mult.x * OutOfBoundsUtils.defaultX * ControllerManager.MapSize;
            this.MinXTarget = -mult.x * OutOfBoundsUtils.defaultX * ControllerManager.MapSize;
            this.MaxYTarget = mult.y * OutOfBoundsUtils.defaultY * ControllerManager.MapSize;
            this.MinYTarget = -mult.y * OutOfBoundsUtils.defaultY * ControllerManager.MapSize;

            base.OnUpdate();
        }
    }
}
