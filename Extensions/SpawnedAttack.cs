using System.Runtime.CompilerServices;

namespace GameModeCollection.Extensions
{
    public class SpawnedAttackAdditionalData
    {
        public bool canDamageChaos = false;
    }
    public static class SpawnedAttackExtensions
    {
        private static readonly ConditionalWeakTable<SpawnedAttack, SpawnedAttackAdditionalData> additionalData = new ConditionalWeakTable<SpawnedAttack, SpawnedAttackAdditionalData>();
        public static SpawnedAttackAdditionalData GetData(this SpawnedAttack instance)
        {
            return additionalData.GetOrCreateValue(instance);
        }
    }
}
