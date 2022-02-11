using GameModeCollection.GameModes.TRT.Roles;
using GameModeCollection.Utils;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using TMPro;
using Photon.Pun;

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

        public static List<IRoleHandler> GetRoleLineup(int N)
        {
            // remove roles for which there are not enough players
            List<IRoleHandler> possibleRoles = RoleHandlers.Values.Where(r => r.MinNumberOfPlayersForRole <= N).ToList();

            // start with the roles that must be present in the lineup
            List<IRoleHandler> lineup = new List<IRoleHandler>() { };

            foreach (IRoleHandler roleHandler in possibleRoles.Where(r => r.MinNumberOfPlayersWithRole > 0))
            {
                for (int _ = 0; _ < roleHandler.MinNumberOfPlayersWithRole; _++)
                {
                    lineup.Add(roleHandler);
                }
            }

            int outerIter = 100;
            while (outerIter > 0)
            {
                outerIter--;

                // now select random roles to fill the rest out
                int remaining = lineup.Count();
                for (int _ = 0; _ < N - remaining; _++)
                {
                    // remove roles where the max has been reached
                    possibleRoles = possibleRoles.Where(r => r.MaxNumberOfPlayersWithRole > lineup.Where(h => h.RoleID == r.RoleID).Count()).ToList();

                    if (possibleRoles.Count() == 0) { break; }

                    lineup.Add(DrawRandomRole(possibleRoles));
                }

                // finally, perform any overwriting
                int iter = 100;
                while (iter > 0 && lineup.Select(r => r.RoleIDsToOverwrite).SelectMany(o => o).Distinct().Intersect(lineup.Select(h => h.RoleID)).Any())
                {
                    iter--;
                    IRoleHandler replacer = lineup.First(r => lineup.Select(h => h.RoleID).Intersect(r.RoleIDsToOverwrite).Any());

                    lineup = lineup.Select(r => replacer.RoleIDsToOverwrite.Contains(r.RoleID) ? replacer : r).ToList();
                }

                // remove anything over the max (which could've occurred due to overwriting), and loop around if there's not enough roles yet
                lineup = lineup.Where(r => r.MaxNumberOfPlayersWithRole >= lineup.Where(h => h.RoleID == r.RoleID).Count()).ToList();

                if (lineup.Count() >= N) { break; }

            }

            // as a failsafe, if this is about to return less than the requested number of roles, pad with the innocent role
            while (lineup.Count() < N)
            {
                lineup.Add(GetHandler(Innocent.RoleAppearance.Name));
            }

            // finally, at least 62.5% of the players should be aligned with the innocents, if this requirement is not met, then
            // replace some of the traitor-aligned roles with innocents
            while ((float)lineup.Count(r => r.RoleAlignment == Alignment.Innocent)/(float)lineup.Count() < 0.625f)
            {
                GameModeCollection.Log("[RoleManager] Not enough innocents. Correcting...");
                int i = lineup.LastIndexOf(lineup.Last(r => r.RoleAlignment == Alignment.Traitor));
                if (i == -1) { break; }
                lineup[i] = DrawRandomRole(RoleHandlers.Values.Where(r => r.RoleAlignment == Alignment.Innocent && r.MinNumberOfPlayersForRole <= N && r.MaxNumberOfPlayersWithRole > lineup.Count(r2 => r2 == r)).ToList());
            }

            return lineup.Take(N).OrderBy(_ => UnityEngine.Random.Range(0f,1f)).ToList();
        }
        public static IRoleHandler DrawRandomRole(List<IRoleHandler> RolesToDrawFrom)
        {
            return RolesToDrawFrom.RandomElementByWeight(r => r.Rarity);
        }

        private static void SetPlayerNameRoleDisplay(Player player, TRT_Role_Appearance role_Appearance, bool hideNickName)
        {
            TextMeshProUGUI nameText = player?.GetComponentInChildren<PlayerName>()?.GetComponent<TextMeshProUGUI>();
            if (nameText is null)
            {
                GameModeCollection.LogWarning($"NAME FOR PLAYER {player?.playerID} IS NULL");
                return;
            }
            string nickName = hideNickName ? "" : (player.GetComponent<PhotonView>()?.Owner?.NickName ?? "");
            if (role_Appearance is null)
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
        public static ITRT_Role GetPlayerRole(Player player)
        {
            return player.GetComponentInChildren<ITRT_Role>();
        }
        public static Alignment? GetPlayerAlignment(Player player)
        {
            return player.GetComponentInChildren<ITRT_Role>()?.Alignment;
        }

        public static void DoRoleDisplay(Player player, bool hideNickNames = true)
        {
            if (player is null) { return; }
            foreach (Player otherPlayer in PlayerManager.instance.players)
            {
                if (otherPlayer.playerID == player.playerID)
                {
                    // always show the player their own role
                    SetPlayerNameRoleDisplay(otherPlayer, GetPlayerRole(otherPlayer)?.Appearance, hideNickNames);
                }
                else
                {
                    SetPlayerNameRoleDisplay(otherPlayer, GetPlayerRole(player)?.Alignment is null ? null : GetPlayerRole(otherPlayer)?.AppearToAlignment(GetPlayerRole(player).Alignment), hideNickNames);
                }
            }
        }

        public static string GetWinningRoleID(Player[] playersRemaining)
        {
            Player[] winners = PlayerManager.instance.players.Where(p => GetPlayerRole(p)?.WinConditionMet(playersRemaining) ?? false).ToArray();
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
