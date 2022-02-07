﻿using RWF.GameModes;
using GameModeCollection.GameModes;
namespace GameModeCollection.GameModeHandlers
{
    public class TRTHandler : RWFGameModeHandler<GM_TRT>
    {
        internal const string GameModeName = "Trouble in Rounds Town";
        internal const string GameModeID = "TroubleInRoundsTown";
        public TRTHandler() : base(
            name: GameModeName,
            gameModeId: GameModeID,
            allowTeams: false,
            pointsToWinRound: 4,
            roundsToWinGame: 4,
            // null values mean RWF's instance values
            playersRequiredToStartGame: null,
            maxPlayers: null,
            maxTeams: null,
            maxClients: null,
            description: $"Trouble in ROUNDS Town.")
        {
            this.Settings.Add(GameModeCollection.ReviveOnCardAddKey, false);
        }
    }
}