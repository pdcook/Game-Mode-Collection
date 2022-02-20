using System.Runtime.CompilerServices;
using System.Linq;
using GameModeCollection.GameModes;

namespace GameModeCollection.Extensions
{
    public class CharacterDataAdditionalData
    {
        public int maxAllowedCards = GM_TRT.BaseMaxCards;
        public float TRT_Karma = 1f;
    }
    public static class CharacterDataExtensions
    {
        private static readonly ConditionalWeakTable<CharacterData, CharacterDataAdditionalData> additionalData = new ConditionalWeakTable<CharacterData, CharacterDataAdditionalData>();
        public static CharacterDataAdditionalData GetData(this CharacterData instance)
        {
            return additionalData.GetOrCreateValue(instance);
        }
        public static void SetMaxCards(this CharacterData instance, int max)
        {
            instance.GetData().maxAllowedCards = max;
        }
        public static int MaxCards(this CharacterData instance)
        {
            return instance.GetData().maxAllowedCards;
        }
        public static bool CanHaveMoreCards(this CharacterData instance)
        {
            return instance.currentCards.Count() < instance.GetData().maxAllowedCards;
        }
        public static float TRT_Karma(this CharacterData instance)
        {
            return instance.GetData().TRT_Karma; 
        }
        public static void TRT_ResetKarma(this CharacterData instance)
        {
            instance.GetData().TRT_Karma = 1f;
        }
        public static void TRT_ChangeKarma(this CharacterData instance, float amount_to_add, float minimum = 0f)
        {
            instance.GetData().TRT_Karma = UnityEngine.Mathf.Clamp(instance.GetData().TRT_Karma + amount_to_add, minimum, 1f);
        }
    }
}
