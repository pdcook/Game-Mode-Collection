using RWF.GameModes;
using GameModeCollection.GameModes;
using UnboundLib.GameModes;
namespace GameModeCollection.GameModeHandlers
{
    public class BombDefusalHandler : RWFGameModeHandler<GM_BombDefusal>
    {
        internal const string GameModeName = "Bomb Defusal";
        internal const string GameModeID = "Bomb Defusal";

        public BombDefusalHandler() : base(
            name: GameModeName,
            gameModeId: GameModeID,
            allowTeams: true,
            pointsToWinRound: 2,
            roundsToWinGame: 5,
            // null values mean RWF's instance values
            playersRequiredToStartGame: null,
            maxPlayers: null,
            maxTeams: 2,
            #if DEBUG
            maxClients: null
            #else
            maxClients: 1
            #endif
            
            )
        {

        }
    }
}
