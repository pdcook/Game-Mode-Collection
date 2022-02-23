using GameModeCollection.GameModes.TRT.Roles;
using GameModeCollection.GameModeHandlers;
using GameModeCollection.Utils;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using TMPro;
using Photon.Pun;
using GameModeCollection.Extensions;
using UnboundLib;
using UnboundLib.Extensions;
using UnboundLib.Utils;
using System.Text.RegularExpressions;

namespace GameModeCollection.GameModes.TRT
{
    public static class RoleManager
    {
        private static bool inited = false;
        private static Dictionary<string, IRoleHandler> RoleHandlers = new Dictionary<string, IRoleHandler>() { };
        private static Dictionary<string, Type> Roles = new Dictionary<string, Type>() { };
        public static ReadOnlyDictionary<string, IRoleHandler> TRTRoleHandlers => new ReadOnlyDictionary<string, IRoleHandler>(RoleHandlers);
        public static ReadOnlyDictionary<string, Type> TRTRoles => new ReadOnlyDictionary<string, Type>(Roles);
        internal static void Init()
        {
            if (inited) { return; }

            inited = true;

            AddRoleHandler<Innocent>(new InnocentRoleHandler());
            AddRoleHandler<Detective>(new DetectiveRoleHandler());
            AddRoleHandler<Glitch>(new GlitchRoleHandler());
            AddRoleHandler<Mercenary>(new MercenaryRoleHandler());
            AddRoleHandler<Phantom>(new PhantomRoleHandler());

            AddRoleHandler<Traitor>(new TraitorRoleHandler());
            AddRoleHandler<Assassin>(new AssassinRoleHandler());
            AddRoleHandler<Hypnotist>(new HypnotistRoleHandler());
            AddRoleHandler<Vampire>(new VampireRoleHandler());
            AddRoleHandler<Zombie>(new ZombieRoleHandler());

            AddRoleHandler<Jester>(new JesterRoleHandler());
            AddRoleHandler<Swapper>(new SwapperRoleHandler());

            AddRoleHandler<Killer>(new KillerRoleHandler());
        }

        public static void AddRoleHandler<TRole>(IRoleHandler handler) where TRole : TRT_Role
        {
            RoleHandlers.Add(handler.RoleID, handler);
            Roles.Add(handler.RoleID, typeof(TRole));
        }
        public static void RemoveRoleHandler(string ID)
        {
            if (RoleHandlers.ContainsKey(ID)) { RoleHandlers.Remove(ID); }
            if (Roles.ContainsKey(ID)) { Roles.Remove(ID); }
        }

        public static IRoleHandler GetHandler(string ID)
        {
            return RoleHandlers[ID];
        }

        public static string GetRoleID(ITRT_Role role)
        {
            if (Roles.Values.Contains(role.GetType()))
            {
                return Roles.First(kv => kv.Value == role.GetType()).Key;
            }
            else
            {
                GameModeCollection.LogError($"[RoleManager] ROLE \"{role}\" NOT FOUND OR NOT REGISTERED");
                return null;
            }
        }

        public static string GetPlayerRoleID(Player player)
        {
            ITRT_Role role = GetPlayerRole(player);
            if (role is null) { return null; }
            if (Roles.Values.Contains(role.GetType()))
            {
                return GetRoleID(role);
            }
            else
            {
                return null;
            }
        }

        public static List<IRoleHandler> GetRoleLineup(int N)
        {
            /*
             * Outline of the method:
             * 
             * - get a list of all possible roles (since some roles require a minimum number of players before they will spawn)
             * - populate the initial lineup with 25% traitor, 12.5% detective (rounded up)
             * - then iterate through the lineup as it stands and randomly see if the role should be replaced
             * ---> if the replacement would change the alignment of the role, check to see if it would make it so that the percentage of innocents would fall below the required level
             * - finally, do any overwriting necessary (e.g. zombies overwrite all traitors)
             */


            // remove roles for which there are not enough players
            List<IRoleHandler> possibleRoles = RoleHandlers.Values.Where(r => r.MinNumberOfPlayersForRole <= N).ToList();

            // get roles that can replace other alignments
            List<IRoleHandler> innocentReplace = possibleRoles.Where(r => r.AlignmentToReplace == Alignment.Innocent).OrderBy(_ => UnityEngine.Random.Range(0f,1f)).ToList();
            List<IRoleHandler> traitorReplace = possibleRoles.Where(r => r.AlignmentToReplace == Alignment.Traitor).OrderBy(_ => UnityEngine.Random.Range(0f,1f)).ToList();

            // start with the roles that must be present in the lineup
            List<IRoleHandler> lineup = Enumerable.Repeat(GetHandler(InnocentRoleHandler.InnocentRoleID), N).ToList();
            int nT;
            if (UnityEngine.Random.Range(0f,1f) < 0.5f)
            {
                nT = UnityEngine.Mathf.CeilToInt(N * TraitorRoleHandler.TraitorRarity);
            }
            else
            {
                nT = UnityEngine.Mathf.Clamp(UnityEngine.Mathf.FloorToInt(N * TraitorRoleHandler.TraitorRarity), 1, int.MaxValue);
            }
            int nD = UnityEngine.Mathf.CeilToInt(N * DetectiveRoleHandler.DetectiveRarity);
            for (int i = 0; i < nT+nD; i++)
            {
                if (i < nT)
                {
                    lineup[i] = GetHandler(TraitorRoleHandler.TraitorRoleID);
                }
                else
                {
                    lineup[i] = GetHandler(DetectiveRoleHandler.DetectiveRoleID);
                }
            }
            // innocent, detective, and traitor do not have maximum amounts, nor can they replace other roles
            // so there is no need to update possibleRoles here

            // see if other roles will replace any
            for (int j = 0; j < innocentReplace.Count(); j++)
            {
                if (innocentReplace[j].RoleAlignment != Alignment.Innocent && ((float)lineup.Count(r => r.RoleAlignment == Alignment.Innocent)-1f) / (float)lineup.Count() < InnocentRoleHandler.MinimumPercInnocent)
                {
                    continue;
                }
                if (UnityEngine.Random.Range(0f, 1f) < innocentReplace[j].Rarity)
                {
                    int i = lineup.IndexOf(GetHandler(InnocentRoleHandler.InnocentRoleID));
                    if (i != -1)
                    {
                        lineup[i] = innocentReplace[j];
                    }
                }
            }
            for (int j = 0; j < traitorReplace.Count(); j++)
            {
                if (UnityEngine.Random.Range(0f, 1f) < traitorReplace[j].Rarity)
                {
                    int i = lineup.IndexOf(GetHandler(TraitorRoleHandler.TraitorRoleID));
                    if (i != -1)
                    {
                        lineup[i] = traitorReplace[j];
                    }
                }
            }

            // finally, perform any overwriting
            int iter = 100;
            while (iter > 0 && lineup.Select(r => r.RoleIDsToOverwrite).SelectMany(o => o).Distinct().Intersect(lineup.Select(h => h.RoleID)).Any())
            {
                iter--;
                IRoleHandler replacer = lineup.FirstOrDefault(r => lineup.Select(h => h.RoleID).Intersect(r.RoleIDsToOverwrite).Any());
                if (replacer is null) { break; }

                lineup = lineup.Select(r => replacer.RoleIDsToOverwrite.Contains(r.RoleID) ? replacer : r).ToList();
            }

            // as a failsafe, if this is about to return less than the requested number of roles, pad with the innocent role
            while (lineup.Count() < N)
            {
                lineup.Add(GetHandler(InnocentRoleHandler.InnocentRoleID));
            }

            return lineup.Take(N).OrderBy(_ => UnityEngine.Random.Range(0f,1f)).ToList();
        }
        public static IRoleHandler DrawRandomRole(List<IRoleHandler> RolesToDrawFrom)
        {
            return RolesToDrawFrom.RandomElementByWeight(r => r.Rarity);
        }
        private static string GetReputability(Player player)
        {
            switch (player.data.TRT_Karma())
            {
                case float k when k >= 0.9f:
                    return GetColoredString("[Reputable]", new Color32(0, 200, 0, 255));
                case float k when (k < 0.9f && k >= 0.8f):
                    return GetColoredString("[Trigger Happy]", new Color32(129, 199, 0, 255));
                case float k when (k < 0.8f && k >= 0.65f):
                    return GetColoredString("[Crude]", new Color32(199, 196, 0, 255));
                case float k when (k < 0.65f && k >= 0.5f):
                    return GetColoredString("[Dangerous]", new Color32(199, 146, 0, 255));
                case float k when k < 0.5f:
                    return GetColoredString("[Liability]", new Color32(200, 0, 0, 255));
                default:
                    return "";
            }
        }

        private static void SetPlayerNameRoleDisplay(Player player, TRT_Role_Appearance role_Appearance, bool hideNickName, bool clear = false)
        {
            TextMeshProUGUI nameText = player?.GetComponentInChildren<PlayerName>()?.GetComponent<TextMeshProUGUI>();
            if (nameText is null)
            {
                GameModeCollection.LogWarning($"NAME FOR PLAYER {player?.playerID} IS NULL");
                return;
            }
            string nickName = hideNickName ? "" : (player.GetComponent<PhotonView>()?.Owner?.NickName ?? "");
            if (!clear)
            {
                string reputability = GetReputability(player);
                if (reputability != "") { nickName = reputability + (nickName == "" ? "" : "\n") + nickName; }
            }
            if (clear || role_Appearance is null)
            {
                nameText.text = nickName;
                nameText.color = new Color(0.6132f, 0.6132f, 0.6132f, 1f);
                nameText.fontStyle = FontStyles.Normal;
            }
            else
            {
                nameText.text = $"[{role_Appearance.Abbr}]{(nickName != "" ? "\n" : "")}{nickName}";
                nameText.color = role_Appearance.Color;
                nameText.fontStyle = FontStyles.Bold;
            }
        }
        public static Alignment? GetPlayerAlignmentAsSeenByOther(Player player, Player other)
        {
            ITRT_Role other_role = GetPlayerRole(other);
            ITRT_Role player_role = GetPlayerRole(player);
            if (other_role is null || player_role is null) { return null; }
            return player_role.AppearToAlignment(other_role.Alignment)?.Alignment;
        }
        public static ITRT_Role GetPlayerRole(Player player)
        {
            return player.GetComponentInChildren<ITRT_Role>();
        }
        public static Alignment? GetPlayerAlignment(Player player)
        {
            return player.GetComponentInChildren<ITRT_Role>()?.Alignment;
        }

        public static void ClearRoleDisplay(Player player, bool hideNickNames = true)
        {
            SetPlayerNameRoleDisplay(player, null, hideNickNames, true); 
        }
        /// <summary>
        /// Do role display from the perspective of the local player for a specific player only
        /// </summary>
        /// <param name="specificPlayer"></param>
        /// <param name="hideNickNames"></param>
        public static void DoRoleDisplaySpecific(Player specificPlayer, bool hideNickNames = true)
        {
            if (specificPlayer is null) { return; }
            ITRT_Role specificRole = GetPlayerRole(specificPlayer);
            Player localPlayer = PlayerManager.instance.players.Find(p => p.data.view.IsMine);
            if (localPlayer.playerID == specificPlayer.playerID)
            {
                // always show the player their own role
                SetPlayerNameRoleDisplay(specificPlayer, specificRole?.Appearance, hideNickNames);
            }
            else
            {
                ITRT_Role localRole = GetPlayerRole(localPlayer);
                SetPlayerNameRoleDisplay(specificPlayer, localRole?.Alignment is null ? null : GetPlayerRole(specificPlayer)?.AppearToAlignment(localRole.Alignment), hideNickNames);
            }
            if (specificRole is null || !specificPlayer.data.view.IsMine) { return; }
            // to the specific player ONLY, do new display stuff
            //UIHandler.instance.DisplayRoundStartText(specificRole.Appearance.Name, specificRole.Appearance.Color, new Vector3(0.5f, 0.8f, 0f));
            //GameModeCollection.instance.ExecuteAfterSeconds(0.5f, () => {
            //    RWF.UIHandlerExtensions.HideRoundStartText(UIHandler.instance);
            //});
            string playerRoleName = GetRoleColoredName(specificRole.Appearance);
            TRTHandler.SendChat(null, $"You are {GetPlayerColorNameAsColoredString(specificPlayer)}, a{((new List<char> {'a', 'e', 'i', 'o', 'u'}).Contains(specificRole.Appearance.Name.ToLower().First()) ? "n" : "")} {playerRoleName}.", true);
            // now do any necessary reporting
            Dictionary<TRT_Role_Appearance, List<string>> rolesAndNames = new Dictionary<TRT_Role_Appearance, List<string>>();
            foreach (Player otherPlayer in PlayerManager.instance.players)
            {
                if (otherPlayer.playerID == specificPlayer.playerID) { continue; }
                if (!(GetPlayerRole(otherPlayer)?.AlertAlignment(specificRole.Alignment) ?? false)) { continue; }
                TRT_Role_Appearance appearAs = GetPlayerRole(otherPlayer)?.AppearToAlignment(specificRole.Alignment);
                if (appearAs is null) { continue; }

                if (!rolesAndNames.ContainsKey(appearAs))
                {
                    rolesAndNames[appearAs] = new List<string>() { };
                }

                rolesAndNames[appearAs].Add(GetPlayerColorNameAsColoredString(otherPlayer));
            }
            foreach (TRT_Role_Appearance roleAppearance in rolesAndNames.Keys)
            {
                string message = "";
                if (roleAppearance.Alignment == specificRole.Alignment)
                {
                    message += "Fellow ";
                }
                message += GetRoleColoredName(roleAppearance);
                if (rolesAndNames[roleAppearance].Count() != 1) { message += "s"; }
                message += ": ";
                string players = string.Join(", ", rolesAndNames[roleAppearance]);
                int seps = Regex.Matches(players, ", ").Count;
                if (seps == 1)
                {
                    players.Replace(", ", " and ");
                }
                else if (seps > 1)
                {
                    int last = players.LastIndexOf(", ");
                    players = players.Remove(last, ", ".Length).Insert(last, ", and ");
                }
                message += players + ".";

                TRTHandler.SendChat(null, message, true);
            }
            // special case for alerting traitors that there is a killer
            if (specificRole.Alignment == Alignment.Traitor && PlayerManager.instance.players.Any(p => !p.data.dead && GetPlayerAlignment(p) == Alignment.Killer))
            {
                TRTHandler.SendChat(null, $"There is a {GetRoleColoredName(Killer.RoleAppearance)}.", true);
            }
            // special case for alerting traitors that there is a glitch
            /*
            if (specificRole.Alignment == Alignment.Traitor && PlayerManager.instance.players.Any(p => !p.data.dead && GetPlayerRoleID(p) == GlitchRoleHandler.GlitchRoleID))
            {
                TRTHandler.SendChat(null, $"There is a {GetRoleColoredName(Glitch.RoleAppearance)}.", true);
            }*/

        }
        public static void DoRoleDisplay(Player player, bool hideNickNames = true)
        {
            if (player is null) { return; }
            ITRT_Role role = GetPlayerRole(player);
            foreach (Player otherPlayer in PlayerManager.instance.players)
            {
                if (otherPlayer.playerID == player.playerID)
                {
                    // always show the player their own role
                    SetPlayerNameRoleDisplay(otherPlayer, role?.Appearance, hideNickNames);
                }
                else
                {
                    SetPlayerNameRoleDisplay(otherPlayer, role?.Alignment is null ? null : GetPlayerRole(otherPlayer)?.AppearToAlignment(role.Alignment), hideNickNames);
                }
            }
            if (role is null) { return; }
            //UIHandler.instance.DisplayRoundStartText(role.Appearance.Name, role.Appearance.Color, new Vector3(0.5f, 0.8f, 0f));
            //GameModeCollection.instance.ExecuteAfterSeconds(0.5f, () => {
            //    RWF.UIHandlerExtensions.HideRoundStartText(UIHandler.instance);
            //});
            string playerRoleName = GetRoleColoredName(role.Appearance);
            TRTHandler.SendChat(null, $"You are {GetPlayerColorNameAsColoredString(player)}, a{((new List<char> {'a', 'e', 'i', 'o', 'u'}).Contains(role.Appearance.Name.ToLower().First()) ? "n" : "")} {playerRoleName}.", true);
            // now do any necessary reporting
            Dictionary<TRT_Role_Appearance, List<string>> rolesAndNames = new Dictionary<TRT_Role_Appearance, List<string>>();
            foreach (Player otherPlayer in PlayerManager.instance.players)
            {
                if (otherPlayer.playerID == player.playerID) { continue; }
                if (!(GetPlayerRole(otherPlayer)?.AlertAlignment(role.Alignment) ?? false)) { continue; }
                TRT_Role_Appearance appearAs = GetPlayerRole(otherPlayer)?.AppearToAlignment(role.Alignment);
                if (appearAs is null) { continue; }

                if (!rolesAndNames.ContainsKey(appearAs))
                {
                    rolesAndNames[appearAs] = new List<string>() { };
                }

                rolesAndNames[appearAs].Add(GetPlayerColorNameAsColoredString(otherPlayer));
            }
            foreach (TRT_Role_Appearance roleAppearance in rolesAndNames.Keys)
            {
                string message = "";
                if (roleAppearance.Alignment == role.Alignment)
                {
                    message += "Fellow ";
                }
                message += GetRoleColoredName(roleAppearance);
                if (rolesAndNames[roleAppearance].Count() != 1) { message += "s"; }
                message += ": ";
                string players = string.Join(", ", rolesAndNames[roleAppearance]);
                int seps = Regex.Matches(players, ", ").Count;
                if (seps == 1)
                {
                    players.Replace(", ", " and ");
                }
                else if (seps > 1)
                {
                    int last = players.LastIndexOf(", ");
                    players = players.Remove(last, ", ".Length).Insert(last, ", and ");
                }
                message += players + ".";

                TRTHandler.SendChat(null, message, true);
            }
            // special case for alerting traitors that there is a killer
            if (role.Alignment == Alignment.Traitor && PlayerManager.instance.players.Any(p => !p.data.dead && GetPlayerAlignment(p) == Alignment.Killer))
            {
                TRTHandler.SendChat(null, $"There is a {GetRoleColoredName(Killer.RoleAppearance)}.", true);
            }
            // special case for alerting traitors that there is a glitch
            /*
            if (role.Alignment == Alignment.Traitor && PlayerManager.instance.players.Any(p => !p.data.dead && GetPlayerRoleID(p) == GlitchRoleHandler.GlitchRoleID))
            {
                TRTHandler.SendChat(null, $"There is a {GetRoleColoredName(Glitch.RoleAppearance)}.", true);
            }*/
        }
        public static string GetColoredString(string str, Color color, bool bold = false)
        {
            string res = $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{str}</color>";
            if (bold) { res = $"<b>{res}</b>"; }
            return res;
        }

        public static string GetPlayerColorNameAsColoredString(Player player)
        {
            return player is null ? "" : $"<b><color=#{ColorUtility.ToHtmlStringRGB(player.GetTeamColors().color)}>{ExtraPlayerSkins.GetTeamColorName(player.colorID())}</color></b>";
        }

        public static string GetRoleColoredName(TRT_Role_Appearance roleAppearance)
        {
            return roleAppearance is null ? "" : $"<b><color={GetRoleColorHTML(roleAppearance)}>{roleAppearance.Name}</color></b>";
        }

        public static string GetRoleColorHTML(TRT_Role_Appearance roleAppearance)
        {
            return roleAppearance is null ? "white" : "#" + ColorUtility.ToHtmlStringRGB(roleAppearance.Color);
        }

        public static string GetWinningRoleID(Player[] players)
        {
            if (players == null) { return null; }
            Player[] winners = PlayerManager.instance.players.Where(p => GetPlayerRole(p)?.WinConditionMet(players.Where(p_ => !p_.data.dead).ToArray()) ?? false).ToArray();
            if (winners is null || winners.Count() == 0)
            {
                return null;
            }
            ITRT_Role[] winningRoles = winners.Select(p => GetPlayerRole(p)).Where(a => a != null).Distinct().ToArray();
            if (winningRoles is null || winningRoles.Count() == 0)
            {
                return null;
            }
            if (winningRoles.Select(r => r.Alignment).Distinct().Count() > 1)
            {
                GameModeCollection.LogError("[GM_TRT] MULTIPLE ALIGNMENTS HAVE THEIR WIN CONDITIONS SATISFIED");
            }
            return GetRoleID(winningRoles.First());
        }

    }
}
