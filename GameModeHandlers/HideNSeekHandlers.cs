using RWF.GameModes;
using GameModeCollection.GameModes;
using UnboundLib.GameModes;
namespace GameModeCollection.GameModeHandlers
{
    public class HideNSeekHandler : RWFGameModeHandler<GM_HideNSeek>
    {
        internal const string GameModeName = "Hide & Seek";
        internal const string GameModeID = "Hide & Seek";

        public override bool OnlineOnly => true;

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
            maxClients: null,
            description: "1/3 of the players are randomly selected every point to be the seeker. Seekers are bright red and have (amount of hiders x 15) seconds to kill all the hiders. Hiders get a effect that makes them weaker. Point are given based on kills or if the time runs out.")
        {

        }
    }
}
