using GameModeCollection.GameModes.TRT.Roles;
using GameModeCollection.Utils;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

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

            return lineup.Take(N).ToList();
        }
        public static IRoleHandler DrawRandomRole(List<IRoleHandler> RolesToDrawFrom)
        {
            return RolesToDrawFrom.RandomElementByWeight(r => r.Rarity);
        }
            
    }
}
