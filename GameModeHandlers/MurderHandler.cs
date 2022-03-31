using GameModeCollection.Extensions;
using GameModeCollection.GameModes;
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
using GameModeCollection.GameModes.Murder;

namespace GameModeCollection.GameModeHandlers
{
    public class MurderHandler : GameModeHandler<GM_Murder>
    {

        public const string ChatName = "<b>[MURDER]</b>";
        private const float MaxVisibleDistance = 10f;
        public const float MaxInspectDistance = 5f;
        private const float SendFreq = 1f;

        public override string Name
        {
            get { return MurderHandler.GameModeName; }
        }

        internal const string GameModeName = "Murder";
        internal const string GameModeID = "GMC_Murder";
        public override bool OnlineOnly => true;
        public override bool AllowTeams => false;
        public override GameSettings Settings { get; protected set; }

        private static float TimeLastSent = -1f;

        private static List<string> PhoneticAlphabet = new List<string>() { "Alfa", "Bravo", "Charlie", "Delta", "Echo", "Foxtrot", "Golf", "Hotel", "India", "Juliet", "Kilo", "Lima", "Mike", "November", "Oscar", "Papa", "Quebec", "Romeo", "Sierra", "Tango", "Uniform", "Victor", "Whiskey", "X-ray", "Yankee", "Zulu" };

        public override UISettings UISettings => new UISettings("One murderer, one detective, the rest must survive.\nGuide available at\nhttps://pdcook.github.io/Game-Mode-Collection/murder.html\nRequires at least 3 players to start.");

        public MurderHandler() : base(gameModeId: GameModeID)
        {
            this.Settings = new GameSettings()
            {
                { "pointsToWinRound", 4},
                { "roundsToWinGame", 4},
                { "playersRequiredToStartGame", UnityEngine.Mathf.Clamp(3, 1, RWFMod.MaxPlayersHardLimit) },
                { "maxPlayers", UnityEngine.Mathf.Clamp(RWFMod.instance.MaxPlayers, 1, RWFMod.MaxPlayersHardLimit) },
                { "maxTeams", UnityEngine.Mathf.Clamp(RWFMod.instance.MaxTeams, 1, RWFMod.MaxColorsHardLimit) },
                { "maxClients", UnityEngine.Mathf.Clamp(RWFMod.instance.MaxClients, 1, RWFMod.MaxPlayersHardLimit) },
                {GameModeCollection.ReviveOnCardAddKey, false }, // do not revive players when they get a card
                {GameModeCollection.CreatePlayerCorpsesKey, true }, // do not hide players when they die, instead make a corpse
                {GameModeCollection.IgnoreGameFeelKey, true }, // do not shake the screen or add chromatic aberration
            };
        }
        internal static void MurderMenu(GameObject menu)
        {
            MenuHandler.CreateText("MURDER OPTIONS", menu, out TextMeshProUGUI _, 50);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
            MenuHandler.CreateSlider("Default map scale", menu, 30, GameModeCollection.MurderDefaultMapScale.Value, 5f, 1f, (val) => { GameModeCollection.MurderDefaultMapScale.Value = val; }, out var _, false);
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
            Player killingPlayer = player.data.lastSourceOfDamage;

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
        public static string GetRandomPlayerName()
        {
            return MurderHandler.PhoneticAlphabet.GetRandom<string>();
        }
        public static string GetPlayerNameAsColoredString(Player player)
        {
            return GetPlayerNameAsColoredString(player.colorID());
        }
        public static string GetPlayerNameAsColoredString(int colorID)
        {
            return $"<b><color=#{ColorUtility.ToHtmlStringRGB(PlayerSkinBank.GetPlayerSkinColors(colorID).color)}>{"TODO"}</color></b>";
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
                    NetworkingManager.RPC(typeof(MurderHandler), nameof(RPCA_TRT_CreateMessage), -1, message);
                }
            }
            else
            {
                if (local)
                {
                    MenuControllerHandler.instance.GetComponent<BetterChat.ChatMonoGameManager>().CreateLocalMessage(">>> ", null, ExtraPlayerSkins.GetTeamColorName(player.colorID()), player.colorID(), message, "");
                }
                else
                {
                    NetworkingManager.RPC(typeof(MurderHandler), nameof(RPCA_TRT_CreateMessage), player.colorID(), message);
                }
            }
        }
        [UnboundRPC]
        private static void RPCA_TRT_CreateMessage(int senderColorID, string message)
        {
            if (senderColorID >= 0)
            {
                MenuControllerHandler.instance.GetComponent<BetterChat.ChatMonoGameManager>().CreateLocalMessage("@", null, ExtraPlayerSkins.GetTeamColorName(senderColorID), senderColorID, message);
            }
            else
            {
                MenuControllerHandler.instance.GetComponent<BetterChat.ChatMonoGameManager>().CreateLocalMessage("", null, ChatName, -1, message, "");
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
        public static void IsMurderer(Player player)
        {
            if (Time.time - TimeLastSent > SendFreq)
            {
                TimeLastSent = Time.time;
                var nearest = GetNearestVisiblePlayer(player, true, MaxVisibleDistance);
                if (nearest != null)
                {
                    SendChat(player, $"{GetPlayerNameAsColoredString(nearest)} is the murderer!");
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
    }
}
