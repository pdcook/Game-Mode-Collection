using MapEmbiggener.Controllers;
using GameModeCollection.GameModes.TRT;
using UnityEngine;
using MapEmbiggener;

namespace GameModeCollection.GameModes.TRT.Controllers
{
    public class TRTBoundsController : BoundsController
    {
        public static string ControllerID => "TRT_BoundsController";
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
            Vector2 mult = TRTMapManager.GetMapBounds(TRTMapManager.GetMapFromMapID(TRTMapManager.CurrentLevel));
            this.MaxXTarget = mult.x * OutOfBoundsUtils.defaultX * ControllerManager.MapSize;
            this.MinXTarget = -mult.x * OutOfBoundsUtils.defaultX * ControllerManager.MapSize;
            this.MaxYTarget = mult.y * OutOfBoundsUtils.defaultY * ControllerManager.MapSize;
            this.MinYTarget = -mult.y * OutOfBoundsUtils.defaultY * ControllerManager.MapSize;

            base.OnUpdate();
        }
    }
}
