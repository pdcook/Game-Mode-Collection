using GameModeCollection.Extensions;
using GameModeCollection.GameModes;
using GameModeCollection.GameModes.TRT;
using GameModeCollection.GameModes.TRT.Roles;
using GameModeCollection.GameModes.TRT.Controllers;
using Photon.Pun;
using RWF;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnboundLib.Extensions;
using UnboundLib.GameModes;
using UnboundLib.Networking;
using UnboundLib.Utils;
using UnityEngine;
using MapsExt;
using UnboundLib.Utils.UI;
using TMPro;

namespace GameModeCollection.GameModeHandlers
{
    public class TRTHandler : GameModeHandler<GM_TRT>
    {

        public const string ChatName = "<b>[TRT]</b>";
        private const float SendFreq = 1f;
        private const float MaxVisibleDistance = 10f;
        public const float MaxInspectDistance = 5f;

        public override string Name
        {
            get { return TRTHandler.GameModeName; }
        }

        internal const string GameModeName = "Trouble in Rounds Town";
        internal const string GameModeID = "TroubleInRoundsTown";
        public override bool OnlineOnly => !GameModeCollection.DEBUG;
        public override bool AllowTeams => false;
        public override GameSettings Settings { get; protected set; }

        private static float TimeLastSent = -1f;

        public override UISettings UISettings => new UISettings("<size=125%>T</size>rouble in <size=125%>R</size>OUNDS <size=125%>T</size>own.\nGuide available at\nhttps://pdcook.github.io/Game-Mode-Collection/trt.html\nRequires at least 4 players to start.");

        public TRTHandler() : base(gameModeId: GameModeID)
        {
            this.Settings = new GameSettings()
            {
                { "pointsToWinRound", 4},
                { "roundsToWinGame", 4},
                { "playersRequiredToStartGame", UnityEngine.Mathf.Clamp(4, 1, RWFMod.MaxPlayersHardLimit) },
                { "maxPlayers", UnityEngine.Mathf.Clamp(RWFMod.instance.MaxPlayers, 1, RWFMod.MaxPlayersHardLimit) },
                { "maxTeams", UnityEngine.Mathf.Clamp(RWFMod.instance.MaxTeams, 1, RWFMod.MaxColorsHardLimit) },
                { "maxClients", UnityEngine.Mathf.Clamp(RWFMod.instance.MaxClients, 1, RWFMod.MaxPlayersHardLimit) },
                {GameModeCollection.ReviveOnCardAddKey, false }, // do not revive players when they get a card
                {GameModeCollection.CreatePlayerCorpsesKey, true }, // do not hide players when they die, instead make a corpse
                {GameModeCollection.IgnoreGameFeelKey, true }, // do not shake the screen or add chromatic aberration
                {GameModeCollection.DisableColliderDamageKey, true }, // physics objects do not deal damage
                {GameModeCollection.DefaultBlockCooldownMultiplierKey, 2f }, // block cooldown is twice as long
                {GameModeCollection.SuffocationDamageEnabledKey, true }, // players take suffocation damage
                {GameModeCollection.HideGunOnDeathKey, true }, // guns are hidden when players die
            };
        }
        internal static void TRTMenu(GameObject menu)
        {
            MenuHandler.CreateText("TROUBLE IN ROUNDS TOWN OPTIONS", menu, out TextMeshProUGUI _, 50);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            MenuHandler.CreateSlider("Default map scale", menu, 30, GameModeCollection.TRTDefaultMapScale.Value, 5f, 1f, (val) => { GameModeCollection.TRTDefaultMapScale.Value = val; } , out var _, false);

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
            // add TRT nameplate component to player
            player.gameObject.GetOrAddComponent<TRTNamePlate>();
            this.GameMode.PlayerJoined(player);
        }

        public override void PlayerDied(Player player, int playersAlive)
        {
            Player killingPlayer = player.data.lastSourceOfDamage;
            killingPlayer?.GetComponent<ITRT_Role>()?.OnKilledPlayer(player);

            player.GetComponent<ITRT_Role>()?.OnKilledByPlayer(killingPlayer);

            PlayerManager.instance.ForEachAlivePlayer(p =>
            {
                RoleManager.GetPlayerRole(p)?.OnAnyPlayerDied(player, PlayerManager.instance.players.Where(pl => !pl.data.dead).Select(pl => RoleManager.GetPlayerRole(pl)).ToArray());
            });

            if (player?.data?.view?.IsMine ?? false)
            {
                TRTShopHandler.CloseAllShops();
            }

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
                message += $"{GetPlayerNameAsColoredString(player)} ";
            }
            message += $"found the body of {GetPlayerNameAsColoredString(body.Player)}, they were a{((new List<char> { 'a', 'e', 'i', 'o', 'u' }).Contains(role.Name.ToLower().First()) ? "n" : "")} {RoleManager.GetRoleColoredName(role)}!";
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
            message += $"searched the body of {GetPlayerNameAsColoredString(body.Player)}, ";
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

        private static bool CanReceiveTraitorChat(int senderID, int receiverID)
        {
            Player sender = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == senderID);
            Player receiver = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == receiverID);
            if (sender is null || receiver is null) { return false; }
            if (sender.data.dead) { return false; } // dead players cannot send messages in traitor chat
            if (RoleManager.GetPlayerAlignment(receiver) != Alignment.Traitor) { return false; } // dead OR alive players that are not traitors cannot see traitor chat (because they could be revived)
            if (PlayerManager.instance.players.Any(p => !p.data.dead && RoleManager.GetPlayerRoleID(p) == GlitchRoleHandler.GlitchRoleID)) { return false; } // if there are any glitches alive, the traitor chat does not work
            if (RoleManager.GetPlayerAlignment(sender) == Alignment.Traitor || RoleManager.GetPlayerAlignment(sender) == Alignment.Chaos) { return true; } // traitors, jesters, and swappers can send messages to the traitor chat
            return false; // default is no.
        }
        private static bool CanSeeTraitorGroup(int playerID)
        {
            Player player = PlayerManager.instance.players.FirstOrDefault(p => p.playerID == playerID);
            if (player is null) { return false; }
            // if the player is a jester or swapper, they can always see the traitor group
            if (RoleManager.GetPlayerAlignment(player) == Alignment.Chaos) { return true; }
            // if there is a Glitch, then traitors cannot see the group until the glitch is killed
            if (PlayerManager.instance.players.Any(p => !p.data.dead && RoleManager.GetPlayerRoleID(p) == GlitchRoleHandler.GlitchRoleID)) { return false; }
            // only traitors can see the traitor group
            if (RoleManager.GetPlayerAlignment(player) == Alignment.Traitor) { return true; }
            return false; // default no.

        }

        public static void InitChatGroups()
        {
            BetterChat.BetterChat.GroupSettings TraitorChatGroup = new BetterChat.BetterChat.GroupSettings(CanReceiveTraitorChat, KeyCode.Y, canSeeGroup: CanSeeTraitorGroup);
            BetterChat.BetterChat.CreateGroup("Traitors", TraitorChatGroup);
        }
        public static void SendPointOverChat(IRoleHandler winningRole)
        {
            SendChat(null, GetColoredString(winningRole.WinMessage, winningRole.WinColor, true), false);
        }
        public static string GetColoredString(string str, Color color, bool bold = false)
        {
            string res = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{str}</color>";
            if (bold) { res = $"<b>{res}</b>"; }
            return res;
        }
        public static void SendChat(Player player, string message, bool local = false)
        {
            if (player is null)
            {
                if (local)
                {
                    MenuControllerHandler.instance.GetComponent<BetterChat.ChatMonoGameManager>().CreateLocalMessage(">>> ", null, ChatName, -1, message, "");
                }
                else
                {
                    NetworkingManager.RPC(typeof(TRTHandler), nameof(RPCA_TRT_CreateMessage), -1, message);
                }
            }
            else
            {
                if (local)
                {
                    MenuControllerHandler.instance.GetComponent<BetterChat.ChatMonoGameManager>().CreateLocalMessage(">>> ", null, player.data.view.Owner.NickName, player.colorID(), message, "");
                }
                else
                {
                    NetworkingManager.RPC(typeof(TRTHandler), nameof(RPCA_TRT_CreateMessage), player.playerID, message);
                }
            }
        }
        [UnboundRPC]
        private static void RPCA_TRT_CreateMessage(int senderPlayerID, string message)
        {
            if (senderPlayerID >= 0)
            {
                Player player = PlayerManager.instance.GetPlayerWithID(senderPlayerID);
                if (player is null) { return; }
                MenuControllerHandler.instance.GetComponent<BetterChat.ChatMonoGameManager>().CreateLocalMessage("@", null, player.data.view.Owner.NickName, senderPlayerID, message);
            }
            else
            {
                MenuControllerHandler.instance.GetComponent<BetterChat.ChatMonoGameManager>().CreateLocalMessage("", null, ChatName, -1, message, "");
            }
        }
        // inspect or interact with a body
        public static void TryInspectBody(Player player, bool interact)
        {
            var nearest = GetNearestVisiblePlayer(player, false, MaxInspectDistance);
            if (nearest != null && nearest.GetComponentInChildren<TRT_Corpse>() != null)
            {
                player.GetComponentInChildren<ITRT_Role>()?.OnInteractWithCorpse(nearest.GetComponentInChildren<TRT_Corpse>(), interact);
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
                    SendChat(player, $"I'm with {GetPlayerNameAsColoredString(nearest)}.");
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
                    SendChat(player, $"{GetPlayerNameAsColoredString(nearest)} is suspicious.");
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
                    SendChat(player, $"{GetPlayerNameAsColoredString(nearest)} is a traitor!");
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
                    SendChat(player, $"{GetPlayerNameAsColoredString(nearest)} is innocent.");
                }
            }
        }
        public static string GetPlayerNameAsColoredString(Player player)
        {
            return player is null ? "" : $"<b><color=#{ColorUtility.ToHtmlStringRGB(player.GetTeamColors().color)}>{player.data.view.Owner.NickName}</color></b>";
        }
    }
}
