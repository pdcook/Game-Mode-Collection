using RWF.GameModes;
using GameModeCollection.GameModes;
using UnboundLib.GameModes;
using System.Linq;
using UnboundLib;
using RWF;

namespace GameModeCollection.GameModeHandlers
{
    public class TRTHandler : GameModeHandler<GM_TRT>
    {
        public override string Name
        {
            get { return TRTHandler.GameModeName; }
        }

        internal const string GameModeName = "Trouble in Rounds Town";
        internal const string GameModeID = "TroubleInRoundsTown";
        public override GameSettings Settings { get; protected set; }

        public TRTHandler() : base(gameModeId: GameModeID)
        {
            this.Settings = new GameSettings()
            {
                { "pointsToWinRound", 4},
                { "roundsToWinGame", 4},
                { "allowTeams", false },
                { "playersRequiredToStartGame", UnityEngine.Mathf.Clamp(4, 1, RWFMod.MaxPlayersHardLimit) },
                { "maxPlayers", UnityEngine.Mathf.Clamp(RWFMod.instance.MaxPlayers, 1, RWFMod.MaxPlayersHardLimit) },
                { "maxTeams", UnityEngine.Mathf.Clamp(RWFMod.instance.MaxTeams, 1, RWFMod.MaxColorsHardLimit) },
                { "maxClients", UnityEngine.Mathf.Clamp(RWFMod.instance.MaxClients, 1, RWFMod.MaxPlayersHardLimit) },
                { "description", "Trouble in ROUNDS Town"},
                { "descriptionFontSize", 30},
                { "videoURL", "https://media.giphy.com/media/lcngwaPCkqFbfhzrsH/giphy.mp4"},
                {GameModeCollection.ReviveOnCardAddKey, false }, // do not revive players when they get a card
                {GameModeCollection.CreatePlayerCorpsesKey, true }, // do not hide players when they die, instead make a corpse
                {GameModeCollection.SeparateChatForDeadPlayersKey, true } // dead players have a separate chat
            };
        }
        public override int[] GetGameWinners()
        {
            if (this.GameMode.roundsPlayed >= (int)this.Settings["roundsToWinGame"])
            {
                return new int[] { 0 };
            }
            else
            {
                return new int[] { };
            }
        }
        public override TeamScore GetTeamScore(int teamID)
        {
            if (this.GetGameWinners().Count() > 0)
            {
                return new TeamScore(0, (int)this.Settings["roundsToWinGame"]);
            }
            else
            {
                return new TeamScore(0, 0);
            }
        }
        public override void SetTeamScore(int teamID, TeamScore score)
        {
            return;
        }

        public override void SetActive(bool active)
        {
            this.GameMode.gameObject.SetActive(active);
        }

        public override void PlayerJoined(Player player)
        {
            this.GameMode.PlayerJoined(player);
        }

        public override void PlayerDied(Player player, int playersAlive)
        {
            this.GameMode.PlayerDied(player, playersAlive);
        }
        public override void StartGame()
        {
            this.GameMode.StartGame();
        }

        public override void ResetGame()
        {
            this.GameMode.ResetMatch();
        }

        public override void ChangeSetting(string name, object value)
        {
            base.ChangeSetting(name, value);

            if (name == "roundsToWinGame")
            {
                UIHandler.instance.InvokeMethod("SetNumberOfRounds", (int)value);
            }
        }
    }
}
