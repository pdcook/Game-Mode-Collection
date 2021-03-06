using Sonigon;
using UnityEngine;
using GameModeCollection.Objects;
using System;
using System.Collections;
using UnboundLib.Utils;

namespace GameModeCollection.Utils
{
    public static class GMCAudio
    {
        // volume is a function of distance, walls between the source and listener, max distance, and min distance
        public const float MinDistance = 5f;
        public const float MaxDistance = 40f;
        public const float CutoffDistance = 40f; // distance at which sounds cannot be heard at all
        public const float WallPenaltyPercent = 0.5f; // each wall cuts off this much percent of the volume 
        public const int MaxWallsCutoff = 3; // number of walls between source and listener after which sounds cannot be heard at all
        public const float DopplerLevel = 0f;
        public const AudioRolloffMode Rolloff = AudioRolloffMode.Logarithmic;

        public static float FalloffByDistance(Vector2 loc1, Vector2 loc2, AudioRolloffMode audioRolloffMode = Rolloff, float minDistance = MinDistance, float maxDistance = MaxDistance, float cutoffDistance = CutoffDistance)
        {
            float dist = Vector2.Distance(loc1, loc2);
            switch (audioRolloffMode)
            {
                case AudioRolloffMode.Logarithmic:
                    return LogarithmicFalloff(dist, minDistance, maxDistance, cutoffDistance);
                case AudioRolloffMode.Linear:
                    return LinearFalloff(dist, minDistance, maxDistance, cutoffDistance);
                case AudioRolloffMode.Custom:
                default:
                    GameModeCollection.LogError("[GMCAudio] Custom audio rolloff mode not supported. Falling back to Logarithmic.");
                    return LogarithmicFalloff(dist, minDistance, maxDistance, cutoffDistance);
            }
        }

        public static float FalloffByWalls(Vector2 loc1, Vector2 loc2, float wallPenaltyPercent = WallPenaltyPercent, int maxWallsCutoff = MaxWallsCutoff, float? dist = null)
        {
            int walls = GetWallsBetween(loc1, loc2, dist);
            if (walls > maxWallsCutoff) { return 0f; }
            return UnityEngine.Mathf.Pow(1f - wallPenaltyPercent, walls);

        }

        public static float Falloff(
            Vector2 loc1, Vector2 loc2,
            AudioRolloffMode audioRolloffMode = Rolloff,
            float minDistance = MinDistance,
            float maxDistance = MaxDistance,
            float cutoffDistance = CutoffDistance,
            float wallPenaltyPercent = WallPenaltyPercent,
            int maxWallsCutoff = MaxWallsCutoff)
        {
            return FalloffByWalls(loc1, loc2, wallPenaltyPercent, maxWallsCutoff) * FalloffByDistance(loc1, loc2, audioRolloffMode, minDistance, maxDistance, cutoffDistance);
        }

        private static int GetWallsBetween(Vector2 loc1, Vector2 loc2, float? dist = null)
        {
            int walls = 0;
            float _dist = dist ?? Vector2.Distance(loc1, loc2);
            RaycastHit2D[] array = Physics2D.RaycastAll(loc1, (loc2 - loc1)/UnityEngine.Mathf.Sqrt(_dist), _dist, PlayerManager.instance.canSeePlayerMask);
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].transform
                    && !array[i].transform.root.GetComponent<SpawnedAttack>()
                    && !array[i].transform.root.GetComponent<Player>()
                    && !array[i].transform.root.GetComponentInChildren<PhysicsItem>()
                    )
                {
                    walls++;
                }
            }
            return walls;
        }
        private static float LogarithmicFalloff(float dist, float minDistance, float maxDistance, float cutoffDistance)
        {
            if (dist < minDistance) { return 1f; }
            else if (dist > cutoffDistance) { return 0f; }
            else if (dist > maxDistance) { return UnityEngine.Mathf.Clamp01(minDistance / maxDistance); }
            return UnityEngine.Mathf.Clamp01(minDistance / dist);
        }
        private static float LinearFalloff(float dist, float minDistance, float maxDistance, float cutoffDistance)
        {
            if (dist < minDistance) { return 1f; }
            if (dist > maxDistance || dist > cutoffDistance) { return 0f; }
            return UnityEngine.Mathf.Clamp01(1f - (dist - minDistance) / (maxDistance - minDistance));
        }
        private static float LogarithmicBlend(float dist, float minDistance)
        {
            if (dist < minDistance) { return 0f; }
            return UnityEngine.Mathf.Clamp01(1f - minDistance / dist);
        }
        private static float LinearBlend(float dist, float minDistance, float maxDistance)
        {
            if (dist < minDistance) { return 0f; }
            if (dist > maxDistance) { return 1f; }
            return UnityEngine.Mathf.Clamp01((dist - minDistance) / (maxDistance - minDistance));
        }
        private static Vector2 Listener
        {
            get
            {
                Player Player = PlayerManager.instance.GetLocalPlayer();
                if (Player is null || Player.data.dead)
                {
                    return MainCam.instance.cam.transform.position;
                }
                else
                {
                    return Player.transform.position;
                }
            }
        }


        public static void ApplyToAudioSource(AudioSource audioSource, Vector2? listener = null, float vol = 1f, bool fadeOut = false, TimeSince? fadeTimer = null, float fadeDuration = 1f)
        {
            audioSource.rolloffMode = Rolloff;
            audioSource.minDistance = MinDistance;
            audioSource.maxDistance = MaxDistance;
            audioSource.dopplerLevel = DopplerLevel;
            Vector2 _listener = listener ?? Listener;
            float distance = Vector2.Distance(audioSource.transform.position, _listener);
            if (distance > CutoffDistance) { audioSource.volume = 0f; }
            else if (fadeOut && fadeTimer.HasValue)
            {
                audioSource.volume = vol * Optionshandler.vol_Master * Optionshandler.vol_Sfx * UnityEngine.Mathf.Clamp01(1f - fadeTimer.Value / fadeDuration);
            }
            else
            {
                audioSource.volume = vol * Optionshandler.vol_Master * Optionshandler.vol_Sfx;
            }
            if (Rolloff == AudioRolloffMode.Logarithmic)
            {
                audioSource.spatialBlend = LogarithmicBlend(distance, MinDistance);
            }
            else
            {
                audioSource.spatialBlend = LinearBlend(distance, MinDistance, MaxDistance);
            }
            audioSource.volume *= FalloffByWalls(audioSource.transform.position, _listener, dist: distance);
        }
    }
}
