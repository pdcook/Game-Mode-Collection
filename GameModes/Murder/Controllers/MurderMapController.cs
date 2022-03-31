using MapEmbiggener.Controllers;
using System.Collections;
using Photon.Pun;
using UnboundLib.GameModes;
using MapsExt;
namespace GameModeCollection.GameModes.Murder.Controllers
{
    public class MurderMapController : MapController
    {
        public static string ControllerID => "Murder_MapController";

        public static float? DefaultMapScale => GameModeCollection.MurderDefaultMapScale.Value;
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
            this.MapSize = MurderMapManager.GetMapScale(map) ?? GameModeCollection.MurderDefaultMapScale.Value;
        }
    }
}
