using RWF.GameModes;
using GameModeCollection.GameModes;
namespace GameModeCollection.GameModeHandlers
{
    public class CrownControlHandler : RWFGameModeHandler<GM_CrownControl>
    {
        internal const string GameModeName = "Crown Control";
        internal const string GameModeID = "Crown Control";
        public CrownControlHandler() : base(
            name: GameModeName,
            gameModeId: GameModeID,
            allowTeams: false,
            pointsToWinRound: 2,
            roundsToWinGame: 3,
            // null values mean RWF's instance values
            playersRequiredToStartGame: null,
            maxPlayers: null,
            maxTeams: null,
            maxClients: null,
            description: $"Free for all. Hold the crown for {UnityEngine.Mathf.RoundToInt(GM_CrownControl.secondsNeededToWin)} seconds to win. Respawns enabled.")
        {

        }
    }
    public class TeamCrownControlHandler : RWFGameModeHandler<GM_CrownControl>
    {
        internal const string GameModeName = "Team Crown Control";
        internal const string GameModeID = "Team Crown Control";
        public TeamCrownControlHandler() : base(
            name: GameModeName,
            gameModeId: GameModeID,
            allowTeams: true,
            pointsToWinRound: 2,
            roundsToWinGame: 5,
            // null values mean RWF's instance values
            playersRequiredToStartGame: null,
            maxPlayers: null,
            maxTeams: null,
            maxClients: null,
            description: $"Help your team hold the crown for {UnityEngine.Mathf.RoundToInt(GM_CrownControl.secondsNeededToWin)} seconds to win. Respawns enabled.")
        {

        }
    }
}
