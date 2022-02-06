using RWF.GameModes;
using GameModeCollection.GameModes;
namespace GameModeCollection.GameModeHandlers
{
    public class DodgeballHandler : RWFGameModeHandler<GM_Dodgeball>
    {
        internal const string GameModeName = "Dodgeball";
        internal const string GameModeID = "Dodgeball";
        public DodgeballHandler() : base(
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
            description: $"Free for all. Be the first to destroy the dodgeball or the last player standing. Enemy damage disabled.")
        {
            this.Settings.Add(GameModeCollection.AllowEnemyDamageKey, false);
        }
    }
    public class TeamDodgeballHandler : RWFGameModeHandler<GM_Dodgeball>
    {
        internal const string GameModeName = "Team Dodgeball";
        internal const string GameModeID = "Team Dodgeball";
        public TeamDodgeballHandler() : base(
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
            description: $"Be the first team to destroy the dodgeball or the last team standing. Enemy damage disabled.")
        {
            this.Settings.Add(GameModeCollection.AllowEnemyDamageKey, false);
        }
    }
}
