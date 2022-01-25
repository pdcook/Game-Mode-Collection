using RWF.GameModes;
using GameModeCollection.GameModes;
namespace GameModeCollection.GameModeHandlers
{
    public class RoundketLeagueHandler : RWFGameModeHandler<GM_RoundketLeague>
    {
        internal const string GameModeName = "Roundket League";
        internal const string GameModeID = "Roundket League";
        public RoundketLeagueHandler() : base(
            name: GameModeName,
            gameModeId: GameModeID,
            allowTeams: true,
            pointsToWinRound: 3,
            roundsToWinGame: 5,
            // null values mean RWF's instance values
            playersRequiredToStartGame: null,
            maxPlayers: null,
            maxTeams: 2,
            maxClients: null)
        {

        }
    }
}
