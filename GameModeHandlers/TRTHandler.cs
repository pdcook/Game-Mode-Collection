using RWF.GameModes;
using GameModeCollection.GameModes;
using GameModeCollection.GameModes.TRT;
using GameModeCollection.GameModes.TRT.Roles;
using UnboundLib.GameModes;
using System.Linq;
using UnboundLib;
using RWF;
using System.Collections.Generic;
using UnityEngine;
using UnboundLib.Utils;
using UnboundLib.Extensions;
using Photon.Pun;

namespace GameModeCollection.GameModeHandlers
{
    public class TRTHandler : GameModeHandler<GM_TRT>
    {
        public const string ChatName = "<b>[TRT]</b>";
        private const float SendFreq = 1f;
        private const float MaxVisibleDistance = 10f;
        private const float MaxInspectDistance = 5f;

        public override string Name
        {
            get { return TRTHandler.GameModeName; }
        }

        internal const string GameModeName = "Trouble in Rounds Town";
        internal const string GameModeID = "TroubleInRoundsTown";
        public override GameSettings Settings { get; protected set; }

        private static float TimeLastSent = -1f;

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
        public static string GetApproximatePlayerColor(Player player)
        {
            return GetApproximatePlayerColor(player.colorID());
        }
        public static string GetApproximatePlayerColor(int colorID)
        {
            int ID = colorID % 8;
            ID = ID < 0 ? ID + 8 : ID;
            switch (ID)
            {
                case 0:
                    return "Orange-ish";
                case 1:
                    return "Blue-ish";
                case 2:
                    return "Red-ish";
                case 3:
                    return "Green-ish";
                case 4:
                    return "Yellow-ish";
                case 5:
                    return "Purple-ish";
                case 6:
                    return "Magenta-ish";
                case 7:
                    return "Blue-Green-ish";
                default:
                    return "Unknown";
            }
        }
        public static string GetApproximatePlayerColorAsColoredString(Player player)
        {
            return GetApproximatePlayerColorAsColoredString(player.colorID());
        }
        public static string GetApproximatePlayerColorAsColoredString(int colorID)
        {
            int ID = colorID % 8;
            ID = ID < 0 ? ID + 8 : ID;
            return $"<b><color=#{ColorUtility.ToHtmlStringRGB(PlayerSkinBank.GetPlayerSkinColors(ID).color)}>{GetApproximatePlayerColor(colorID)}</color></b>";
        }

        public static Player GetNearestVisiblePlayer(Player player, bool? alive = true, float maxDistance = float.PositiveInfinity)
        {
            List<Player> players = PlayerManager.instance.players.Where(p => p.playerID != player.playerID && (alive is null || p.data.dead == !(bool)alive)).OrderBy(p => Vector2.Distance(player.transform.position, p.transform.position)).ToList();

            foreach (var other_player in players)
            {
                CanSeeInfo canSee = PlayerManager.instance.CanSeePlayer(player.transform.position, other_player);
                if (canSee.canSee)
                {
                    if (Vector2.Distance(player.transform.position, other_player.transform.position) <= maxDistance) { return other_player; }
                    else { return null; }
                }
            }
            return null;
        }
        public static void PlayerIDBody(Player player, TRT_Corpse body, bool alreadyIDed, bool alreadyInvestigated)
        {
            TRT_Role_Appearance role = RoleManager.GetPlayerRole(body.Player)?.Appearance;
            if (role is null) { return; }
            string message = "";
            if (alreadyIDed)
            {
                message += "You ";
            }
            else
            {
                message += $"{GetPlayerColorNameAsColoredString(player)} ";
            }
            message += $"found the body of {GetPlayerColorNameAsColoredString(body.Player)}, they were a{((new List<char> { 'a', 'e', 'i', 'o', 'u' }).Contains(role.Name.ToLower().First()) ? "n" : "")} {RoleManager.GetRoleColoredName(role)}!";
            SendChat(null, message, alreadyIDed);

            if (alreadyInvestigated)
            {
                PlayerInvestigateBody(player, body, alreadyInvestigated);
            }
        }
        public static void PlayerInvestigateBody(Player player, TRT_Corpse body, bool alreadyInvestigated)
        {
            string message = "";
            if (alreadyInvestigated)
            {
                message += "You ";
            }
            else
            {
                message += $"The {RoleManager.GetRoleColoredName(Detective.RoleAppearance)} ";
            }
            message += $"searched the body of {GetPlayerColorNameAsColoredString(body.Player)}, ";
            int ageInMinutes = UnityEngine.Mathf.RoundToInt((Time.realtimeSinceStartup - body.TimeOfDeath) / 60f);
            message += "they died ";
            if (ageInMinutes == 0)
            {
                message += "less than a minute ago.";
            }
            else if (ageInMinutes == 1)
            {
                message += $"about {ageInMinutes} minute ago.";
            }
            else
            {
                message += $"about {ageInMinutes} minutes ago.";
            }
            if (body.LastShot != null)
            {
                message += $" The last player they shot was {GetApproximatePlayerColorAsColoredString(body.LastShot)}!";
            }
            else if (body.Killer != null)
            {
                message += $" They were killed in cold blood by someone {GetApproximatePlayerColorAsColoredString(body.Killer)}!";
            }
            SendChat(null, message, alreadyInvestigated);

        }
        public static void SendChat(Player player, string message, bool local = false)
        {
            if (player is null)
            {
                if (local)
                {
                    MenuControllerHandler.instance.GetComponent<BetterChat.ChatMonoGameManager>().CreateLocalMessage(ChatName, -1, message);
                }
                else
                {
                    MenuControllerHandler.instance.GetComponent<PhotonView>().RPC("RPCA_CreateMessage", RpcTarget.All, ChatName, -1, message);
                }
            }
            else
            {
                if (local)
                {
                    MenuControllerHandler.instance.GetComponent<BetterChat.ChatMonoGameManager>().CreateLocalMessage(ExtraPlayerSkins.GetTeamColorName(player.colorID()), player.colorID(), message);
                }
                else
                {
                    MenuControllerHandler.instance.GetComponent<PhotonView>().RPC("RPCA_CreateMessage", RpcTarget.All, ExtraPlayerSkins.GetTeamColorName(player.colorID()), player.colorID(), message);
                }
            }
        }
        private static string GetPlayerColorNameAsColoredString(Player player)
        {
            return player is null ? "" : $"<b><color=#{ColorUtility.ToHtmlStringRGB(player.GetTeamColors().color)}>{ExtraPlayerSkins.GetTeamColorName(player.colorID())}</color></b>";
        }
        public static void TryInspectBody(Player player)
        {
            var nearest = GetNearestVisiblePlayer(player, false, MaxInspectDistance);
            if (nearest != null && nearest.GetComponentInChildren<TRT_Corpse>() != null)
            {
                player.GetComponentInChildren<ITRT_Role>()?.OnInteractWithCorpse(nearest.GetComponentInChildren<TRT_Corpse>());
            }
        }
        public static void ImWith(Player player)
        {
            if (Time.time - TimeLastSent > SendFreq)
            {
                TimeLastSent = Time.time;
                var nearest = GetNearestVisiblePlayer(player, true, MaxVisibleDistance);
                if (nearest != null)
                {
                    SendChat(player, $"I'm with {GetPlayerColorNameAsColoredString(nearest)}.");
                }
            }
        }
        public static void IsSuspect(Player player)
        {
            if (Time.time - TimeLastSent > SendFreq)
            {
                TimeLastSent = Time.time;
                var nearest = GetNearestVisiblePlayer(player, true, MaxVisibleDistance);
                if (nearest != null)
                {
                    SendChat(player, $"{GetPlayerColorNameAsColoredString(nearest)} is suspicious.");
                }
            }
        }
        public static void IsTraitor(Player player)
        {
            if (Time.time - TimeLastSent > SendFreq)
            {
                TimeLastSent = Time.time;
                var nearest = GetNearestVisiblePlayer(player, true, MaxVisibleDistance);
                if (nearest != null)
                {
                    SendChat(player, $"{GetPlayerColorNameAsColoredString(nearest)} is a traitor!");
                }
            }
        }
        public static void IsInnocent(Player player)
        {
            if (Time.time - TimeLastSent > SendFreq)
            {
                TimeLastSent = Time.time;
                var nearest = GetNearestVisiblePlayer(player, true, MaxVisibleDistance);
                if (nearest != null)
                {
                    SendChat(player, $"{GetPlayerColorNameAsColoredString(nearest)} is innocent.");
                }
            }
        }
    }
}
