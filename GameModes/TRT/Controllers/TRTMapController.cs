using MapEmbiggener.Controllers;
using System.Collections;
using Photon.Pun;
using UnboundLib.GameModes;
using MapsExt;
namespace GameModeCollection.GameModes.TRT.Controllers
{
    public class TRTMapController : MapController
    {
        public static string ControllerID => "TRT_MapController";

        public static float? DefaultMapScale => GameModeCollection.TRTDefaultMapScale.Value;
        public override void ReadSyncedData()
        {
        }
        public override void SetDataToSync()
        {
        }
        public override bool SyncDataNow()
        {
            return true;
        }

        public override IEnumerator OnGameStart(IGameModeHandler gm)
        {
            if (PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient || PhotonNetwork.CurrentRoom == null)
            {
                this.MapSize = DefaultMapScale;
            }

            return base.OnGameStart(gm);
        }
        public void SetUpcomingMap(CustomMap map)
        {
            this.MapSize = TRTMapManager.GetMapScale(map) ?? GameModeCollection.TRTDefaultMapScale.Value;
        }
    }
}
