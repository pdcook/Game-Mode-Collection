using RWF.GameModes;
using GameModeCollection.GameModes;
using UnboundLib.GameModes;
namespace GameModeCollection.GameModeHandlers
{
    public class HideNSeekHandler : RWFGameModeHandler<GM_HideNSeek>
    {
        internal const string GameModeName = "Hide & Seek";
        internal const string GameModeID = "Hide & Seek";

        public HideNSeekHandler() : base(
            name: GameModeName,
            gameModeId: GameModeID,
            allowTeams: false,
            pointsToWinRound: 4,
            roundsToWinGame: 3,
            // null values mean RWF's instance values
            playersRequiredToStartGame: 3,
            maxPlayers: null,
            maxTeams: null,
            maxClients: null)
        {

        }
    }
}
