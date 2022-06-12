using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using GameModeCollection.GameModes.TRT.RoundEvents;
using GameModeCollection.Extensions;
namespace GameModeCollection.GameModes.TRT
{
    public static class RoundSummary
    {
        /// The class which handles:
        /// - tracking the info of the current round
        /// - displaying round summaries
        /// - managing RoundEvents

        /// Tracked Information:
        /// - all kills (killer and victim)
        /// - all player damage (source, target, amount)
        /// - custom events registered by key

        // custom events
        private static Dictionary<string, IRoundEvent> _CustomEvents = new Dictionary<string, IRoundEvent>();
        public static ReadOnlyDictionary<string, IRoundEvent> CustomEvents { get; } = new ReadOnlyDictionary<string, IRoundEvent>(_CustomEvents);
        public static void RegisterEvent(string ID, IRoundEvent roundEvent)
        {
            if (_CustomEvents.ContainsKey(ID))
            {
                GameModeCollection.LogError("[RoundSummary] Custom RoundEvent ID already exists: " + ID);
            }
            else
            {
                _CustomEvents.Add(ID, roundEvent);
            }
        }
        // kills by each playerID (kills[killerID] = {victimID, victimID, ...})
        private static Dictionary<int, List<int>> Kills = new Dictionary<int, List<int>>() { };
        public static List<int> GetPlayerKills(int playerID)
        {
            if (Kills.ContainsKey(playerID))
            {
                return Kills[playerID];
            }
            return new List<int>();
        }
        // teamkills by each playerID (TeamKills[killerID] = {victimID, victimID, ...})
        private static Dictionary<int, List<int>> TeamKills = new Dictionary<int, List<int>>() { };
        public static List<int> GetPlayerTeamKills(int playerID)
        {
            if (TeamKills.ContainsKey(playerID))
            {
                return TeamKills[playerID];
            }
            return new List<int>();
        }
        // EnemyKills by each playerID (EnemyKills[killerID] = {victimID, victimID, ...})
        private static Dictionary<int, List<int>> EnemyKills = new Dictionary<int, List<int>>() { };
        public static List<int> GetPlayerEnemyKills(int playerID)
        {
            if (EnemyKills.ContainsKey(playerID))
            {
                return EnemyKills[playerID];
            }
            return new List<int>();
        }

        // damage from each playerID (DamageDealt[playerID] = amount)
        private static Dictionary<int, float> DamageDealt = new Dictionary<int, float>() { };
        public static float GetPlayerDamageDealt(int playerID)
        {
            if (DamageDealt.ContainsKey(playerID))
            {
                return DamageDealt[playerID];
            }
            return 0;
        }
        // damage to each playerID (DamageTaken[playerID] = amount)
        private static Dictionary<int, float> DamageTaken = new Dictionary<int, float>() { };
        public static float GetPlayerDamageTaken(int playerID)
        {
            if (DamageTaken.ContainsKey(playerID))
            {
                return DamageTaken[playerID];
            }
            return 0;
        }
        // damage from each playerID to enemies (DamageDealtToEnemies[playerID] = amount)
        private static Dictionary<int, float> DamageDealtToEnemies = new Dictionary<int, float>() { };
        public static float GetPlayerDamageDealtToEnemies(int playerID)
        {
            if (DamageDealtToEnemies.ContainsKey(playerID))
            {
                return DamageDealtToEnemies[playerID];
            }
            return 0;
        }
        // damage from each playerID to allies (DamageDealtToAllies[playerID] = amount)
        private static Dictionary<int, float> DamageDealtToAllies = new Dictionary<int, float>() { };
        public static float GetPlayerDamageDealtToAllies(int playerID)
        {
            if (DamageDealtToAllies.ContainsKey(playerID))
            {
                return DamageDealtToAllies[playerID];
            }
            return 0;
        }
        // damage from each playerID to themselves (DamageDealtToSelf[playerID] = amount)
        private static Dictionary<int, float> DamageDealtToSelf = new Dictionary<int, float>() { };
        public static float GetPlayerDamageDealtToSelf(int playerID)
        {
            if (DamageDealtToSelf.ContainsKey(playerID))
            {
                return DamageDealtToSelf[playerID];
            }
            return 0;
        }

        // custom events by key (Events[playerID][key] = object)
        private static Dictionary<int, Dictionary<string, IRoundEvent>> Events = new Dictionary<int, Dictionary<string, IRoundEvent>>() { };
        public static IRoundEvent GetPlayerEvent(int playerID, string ID)
        {
            if (Events.ContainsKey(playerID))
            {
                if (Events[playerID].ContainsKey(ID))
                {
                    return Events[playerID][ID];
                }
            }
            return null;
        }
        public static IRoundEvent GetPlayerHighestPriorityEvent(int playerID)
        {
            if (Events.ContainsKey(playerID))
            {
                return Events[playerID].Values.OrderByDescending(x => x.Priority).FirstOrDefault() ?? (IRoundEvent)(new BlankEvent());
            }
            return (IRoundEvent)(new BlankEvent());
        }

        public static void ResetAll()
        {
            Kills.Clear();
            TeamKills.Clear();
            EnemyKills.Clear();
            DamageDealt.Clear();
            DamageTaken.Clear();
            DamageDealtToEnemies.Clear();
            DamageDealtToAllies.Clear();
            DamageDealtToSelf.Clear();
            Events.Clear();
        }

        public static void LogDamage(Player source, Player receiver, float damage)
        {
            if (source != null)
            {
                if (DamageDealt.ContainsKey(source.playerID))
                {
                    DamageDealt[source.playerID] += damage;
                }
                else
                {
                    DamageDealt.Add(source.playerID, damage);
                }
            }
            if (receiver != null)
            {
                if (DamageTaken.ContainsKey(receiver.playerID))
                {
                    DamageTaken[receiver.playerID] += damage;
                }
                else
                {
                    DamageTaken.Add(receiver.playerID, damage);
                }
            }
            if (source != null && receiver != null)
            {
                if (RoleManager.GetPlayerAlignment(source) == RoleManager.GetPlayerAlignment(receiver))
                {
                    if (DamageDealtToAllies.ContainsKey(source.playerID))
                    {
                        DamageDealtToAllies[source.playerID] += damage;
                    }
                    else
                    {
                        DamageDealtToAllies.Add(source.playerID, damage);
                    }

                    // log self-damage
                    if (source.playerID == receiver.playerID)
                    {
                        if (DamageDealtToSelf.ContainsKey(source.playerID))
                        {
                            DamageDealtToSelf[source.playerID] += damage;
                        }
                        else
                        {
                            DamageDealtToSelf.Add(source.playerID, damage);
                        }
                    }
                }
                else
                {
                    if (DamageDealtToEnemies.ContainsKey(source.playerID))
                    {
                        DamageDealtToEnemies[source.playerID] += damage;
                    }
                    else
                    {
                        DamageDealtToEnemies.Add(source.playerID, damage);
                    }
                }
            }
        }
        public static void LogKill(Player source, Player receiver)
        {
            if (source != null)
            {
                if (Kills.ContainsKey(source.playerID))
                {
                    Kills[source.playerID].Add(receiver.playerID);
                }
                else
                {
                    Kills.Add(source.playerID, new List<int>() { receiver.playerID });
                }
            }
            if (receiver != null)
            {
                if (EnemyKills.ContainsKey(receiver.playerID))
                {
                    EnemyKills[receiver.playerID].Add(source.playerID);
                }
                else
                {
                    EnemyKills.Add(receiver.playerID, new List<int>() { source.playerID });
                }
            }
            if (source != null && receiver != null)
            {
                if (RoleManager.GetPlayerAlignment(source) == RoleManager.GetPlayerAlignment(receiver))
                {
                    if (TeamKills.ContainsKey(source.playerID))
                    {
                        TeamKills[source.playerID].Add(receiver.playerID);
                    }
                    else
                    {
                        TeamKills.Add(source.playerID, new List<int>() { receiver.playerID });
                    }
                }

                // log suicides
                if (source.playerID == receiver.playerID)
                {
                    LogEvent(SuicideEvent.ID, source.playerID);
                }
            }
        }
        public static void LogEvent(string ID, int playerID, params object[] args)
        {
            if (_CustomEvents.ContainsKey(ID))
            {
                if (!Events.ContainsKey(playerID))
                {
                    Events.Add(playerID, new Dictionary<string, IRoundEvent>());
                }

                if (!Events[playerID].ContainsKey(ID))
                {
                    Events[playerID].Add(ID, (IRoundEvent)Activator.CreateInstance(_CustomEvents[ID].GetType()));
                }

                try
                {
                    Events[playerID][ID].LogEvent(playerID, args);
                }
                catch (Exception e)
                {
                    GameModeCollection.LogError($"[TRT Round Summary] Failed to log event {ID} for player {playerID}. Full exception follows.");
                    GameModeCollection.LogError(e);
                }
            }
        }
        public const string KILLS_KEY = "Kills";
        public const string TEAM_KILLS_KEY = "TeamKills";
        public const string ENEMY_KILLS_KEY = "EnemyKills";
        public const string DAMAGE_DEALT_KEY = "DamageDealt";
        public const string DAMAGE_TAKEN_KEY = "DamageTaken";
        public const string DAMAGE_DEALT_TO_ENEMIES_KEY = "DamageDealtToEnemies";
        public const string DAMAGE_DEALT_TO_ALLIES_KEY = "DamageDealtToAllies";
        public const string DAMAGE_DEALT_TO_SELF_KEY = "DamageDealtToSelf";
        public const string EVENTS_KEY = "Events";

        public static Dictionary<string, string> GetPlayerRoundSummary(int playerID)
        {
            return new Dictionary<string, string>()
            {
                { KILLS_KEY, GetPlayerKills(playerID).Count().ToString("N0") },
                { TEAM_KILLS_KEY, GetPlayerTeamKills(playerID).Count().ToString("N0") },
                { ENEMY_KILLS_KEY, GetPlayerEnemyKills(playerID).Count().ToString("N0") },
                { DAMAGE_DEALT_KEY, GetPlayerDamageDealt(playerID).ToString("N0") },
                { DAMAGE_TAKEN_KEY, GetPlayerDamageTaken(playerID).ToString("N0") },
                { DAMAGE_DEALT_TO_ENEMIES_KEY, GetPlayerDamageDealtToEnemies(playerID).ToString("N0") },
                { DAMAGE_DEALT_TO_ALLIES_KEY, GetPlayerDamageDealtToAllies(playerID).ToString("N0") },
                { DAMAGE_DEALT_TO_SELF_KEY, GetPlayerDamageDealtToSelf(playerID).ToString("N0") },
                { EVENTS_KEY, GetPlayerHighestPriorityEvent(playerID).EventMessage() }
            };
        }

        public static void LogOutRoundEnd()
        {
            if (!GameModeCollection.DEBUG) { return; }
            PlayerManager.instance.ForEachPlayer(p =>
            {
                GameModeCollection.Log($"PLAYER {p.playerID} ROUND SUMMARY");
                foreach (KeyValuePair<string, string> kv in GetPlayerRoundSummary(p.playerID))
                {
                    GameModeCollection.Log($"P{p.playerID} | {kv.Key}: {kv.Value}");
                }
            });
        }
    }
}
