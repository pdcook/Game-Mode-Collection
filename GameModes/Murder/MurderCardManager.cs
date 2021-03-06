using GameModeCollection.GameModes;
using GameModeCollection.Objects;
using GameModeCollection.GMCObjects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnboundLib;
using UnboundLib.Networking;
using Photon.Pun;
using MapEmbiggener;
using GameModeCollection.Utils;
using System.Collections;
using GameModeCollection.GameModes.Murder.Cards;
using CardChoiceSpawnUniqueCardPatch.CustomCategories;
using UnboundLib.Cards;
namespace GameModeCollection.GameModes.Murder
{
    public static class MurderCardManager
    {
        private const int MaxAttempts = 100;
        private const int NumCols = 32;
        private const int NumRows = 20;
        private static readonly LayerMask GroundMask = (LayerMask)LayerMask.GetMask(new string[] { "Default", "IgnorePlayer", "IgnoreMap" });
        private const float GroundOffset = 1f;
        private const float Eps = 1.5f;
        private const float Range = 2f;
        private const float MaxDistanceAway = 10f;

        public static IEnumerator SpawnClues(int N, float health, bool requireInteract)
        {
            if (!PhotonNetwork.IsMasterClient) { yield break; }

            CardSpawnPoint[] cardSpawns = MapManager.instance?.currentMap?.Map?.gameObject?.GetComponentsInChildren<CardSpawnPoint>(true);
            List<Vector2> spawnPoints = (cardSpawns is null || cardSpawns.Count() == 0) ? null : cardSpawns.Select(c => (Vector2)c.transform.position).OrderBy(_ => UnityEngine.Random.Range(0f,1f)).ToList();
            if (spawnPoints is null)
            {
                spawnPoints = GetDefaultPoints(MurderCardManager.Eps).OrderBy(_ => UnityEngine.Random.Range(0f,1f)).ToList();
            }

            for (int i = 0; i < N; i++)
            {
                int j = Mod(i, spawnPoints.Count());
                yield return CardItem.MakeCardItem(ClueCard.clueCard, GetNearbyValidPosition(spawnPoints[j]), Quaternion.identity, maxHealth: health, requireInteract: requireInteract);
            }
        }
        public static void RemoveAllCardItems()
        {
            CardItemHandler.Instance.DestroyAllCardItems();
        }
        private static int Mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }
        private static List<Vector2> GetDefaultPoints(float eps = 0f)
        {
            var points = new List<Vector2>() { };

            Vector2 min = new Vector2(OutOfBoundsUtils.minX, OutOfBoundsUtils.minY);
            Vector2 max = new Vector2(OutOfBoundsUtils.maxX, OutOfBoundsUtils.maxY);

            float dx = (max - min).x / (float)MurderCardManager.NumCols;
            float dy = (max - min).y / (float)MurderCardManager.NumRows;

            Vector2 point;
            Vector2 groundedPoint;

            // Add the basic grid, projected to the nearest ground, only add if it's successfully grounded and is not within eps of any other point
            for (float i = 0.5f; i < MurderCardManager.NumCols; i++)
            {
                for (float j = 0.5f; j < MurderCardManager.NumRows; j++)
                {
                    point = min + new Vector2(i * dx, j * dy);
                    groundedPoint = CastToGround(point, out bool grounded);
                    if (grounded && !points.Where(v => Vector2.Distance(v, groundedPoint) <= eps).Any())
                    {
                        points.Add(groundedPoint);
                    }
                }
            }

            return points;
        }

        private static Vector2 CastToGround(Vector2 position)
        {
            var hit = Physics2D.Raycast(position, Vector2.down, 1000f, MurderCardManager.GroundMask);
            return hit.transform
                ? position + Vector2.down * (hit.distance - MurderCardManager.GroundOffset)
                : position;
        }

        private static Vector2 CastToGround(Vector2 position, out bool success)
        {
            var hit = Physics2D.Raycast(position, Vector2.down, 1000f, MurderCardManager.GroundMask);

            if (!hit.transform || hit.distance <= MurderCardManager.Eps)
            {
                success = false;
                return position;
            }

            success = true;
            return position + Vector2.down * hit.distance + hit.normal * MurderCardManager.GroundOffset;
        }

        private static bool IsValidPosition(Vector2 position, out RaycastHit2D raycastHit2D)
        {

            raycastHit2D = Physics2D.Raycast(position, Vector2.down, MurderCardManager.Range, MurderCardManager.GroundMask);

            // Check if point is inside the bounds
            Vector3 point = OutOfBoundsUtils.InverseGetPoint(position);
            if (point.x <= 0f || point.x >= 1f || point.y <= 0f || point.y >= 1f)
            {
                return false;
            }

            bool hitNearby = raycastHit2D.transform && raycastHit2D.distance > 0.1f;
            bool hitDamageBox = raycastHit2D.collider && raycastHit2D.collider.GetComponent<DamageBox>();

            return hitNearby && !hitDamageBox;
        }

        private static Vector2 GetNearbyValidPosition(Vector2 position, bool requireLOS = true, List<Vector2> avoidPoints = null)
        {
            if (Vector2.Distance(position, CastToGround(position)) > MurderCardManager.MaxDistanceAway)
            {
                return position + RandomUtils.ClippedGaussianVector2(-MurderCardManager.MaxDistanceAway, -MurderCardManager.MaxDistanceAway, MurderCardManager.MaxDistanceAway, MurderCardManager.MaxDistanceAway);
            }

            for (int i = 0; i < MurderCardManager.MaxAttempts; i++)
            {
                var newPosition = CastToGround(position + RandomUtils.ClippedGaussianVector2(-MurderCardManager.MaxDistanceAway, -MurderCardManager.MaxDistanceAway, MurderCardManager.MaxDistanceAway, MurderCardManager.MaxDistanceAway));
                float dist = (avoidPoints == null || avoidPoints.Count() == 0) ? Vector2.Distance(newPosition, position) : avoidPoints.Select(v => Vector2.Distance(v, newPosition)).Max();

                if (IsValidPosition(newPosition, out var _) && dist <= MurderCardManager.MaxDistanceAway)
                {
                    // Check for line-of-sight if required
                    if (requireLOS)
                    {
                        var dir = newPosition - position;

                        if (Physics2D.Raycast(position, dir, dist, MurderCardManager.GroundMask))
                        {
                            // The ray hit something, and therefore there is no line-of-sight, so try again
                            continue;
                        }
                    }

                    return newPosition;
                }
            }

            // If we required LOS and it failed, try again without
            if (requireLOS)
            {
                return GetNearbyValidPosition(position, false, avoidPoints);
            }
            // If we required avoiding points, but not LOS, try it again with no requirements
            else if (avoidPoints != null)
            {
                return GetNearbyValidPosition(position, false, null);
            }

            // If all else fails, just return a random valid position
            return RandomValidPosition();
        }

        private static Vector2 RandomValidPosition()
        {
            for (int i = 0; i < MurderCardManager.MaxAttempts; i++)
            {
                var position = OutOfBoundsUtils.GetPoint(new Vector3( UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 0f));
                if (IsValidPosition(position, out var _))
                {
                    return position;
                }
            }
            return Vector2.zero;
        }
    }
}
